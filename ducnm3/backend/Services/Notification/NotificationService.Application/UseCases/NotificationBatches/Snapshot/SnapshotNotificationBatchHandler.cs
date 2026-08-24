// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Snapshot/SnapshotNotificationBatchHandler.cs
// Mục đích: Đọc student theo page, lưu snapshot recipient idempotent và phát các command dispatch theo concurrency cấu hình.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Services.Students;
using NotificationService.Application.UseCases.NotificationBatches;

namespace NotificationService.Application.UseCases.NotificationBatches.Snapshot;

public sealed class SnapshotNotificationBatchHandler(
    IStudentRecipientClient studentRecipientClient,
    INotificationBatchRepository repository,
    ICommandSender commandSender,
    NotificationBatchProcessingOptions options)
{
    public async Task HandleAsync(
        SnapshotNotificationBatchV1 command,
        CancellationToken cancellationToken)
    {
        // Repository chuyển batch PENDING sang SNAPSHOTTING (nếu phù hợp) và trả quyết định cho lần xử lý này.
        // Quyết định giúp message redelivery không đọc/lưu recipient lần thứ hai khi snapshot đã hoàn tất.
        var work = await repository.PrepareSnapshotAsync(command.BatchId, cancellationToken);
        if (work.ShouldReadRecipients)
        {
            try
            {
                if (work.SourceBatchId is not null)
                {
                    // Retry batch không gọi Student Service: chỉ sao chép các recipient FAILED của batch gốc thành PENDING.
                    await repository.CopyFailedRecipientsAsync(
                        command.BatchId, work.SourceBatchId.Value, cancellationToken);
                }
                else
                {
                    // requestedCount là giới hạn tổng recipient, không phải kích thước từng page Student Service.
                    // ExistingRecipientCount cho phép resume/redelivery tiếp tục đúng quota đã có trong database.
                    uint? remaining = work.RequestedCount is { } requestedCount
                        ? requestedCount > work.ExistingRecipientCount
                            ? requestedCount - work.ExistingRecipientCount
                            : 0
                        : null;
                    await foreach (var page in studentRecipientClient
                        .GetActiveStudentIdPagesAsync(cancellationToken)
                        .WithCancellation(cancellationToken))
                    {
                        if (remaining == 0)
                        {
                            // Đã đủ số recipient được yêu cầu nên không gọi page sau.
                            break;
                        }

                        // Loại duplicate trong page trước khi ghi. Repository dùng INSERT IGNORE/unique constraint để
                        // tiếp tục chống trùng giữa page hoặc khi message bị redeliver.
                        var candidates = page.Distinct().ToArray();
                        if (remaining is not null)
                        {
                            // Cắt page cuối để tổng recipient không vượt requestedCount.
                            candidates = candidates.Take(checked((int)remaining.Value)).ToArray();
                        }
                        // inserted là số row thực sự mới được lưu; chỉ số này mới được trừ khỏi quota còn lại.
                        var inserted = await repository.AppendSnapshotPageAsync(
                            command.BatchId,
                            candidates,
                            cancellationToken);
                        if (remaining is not null)
                        {
                            remaining -= checked((uint)inserted);
                        }
                    }
                }

                // Đếm snapshot thực tế, chuyển batch sang trạng thái sẵn sàng dispatch và cho biết có cần phát dispatch hay không.
                if (!await repository.CompleteSnapshotAsync(command.BatchId, cancellationToken))
                {
                    // Batch có thể đã được một lần consume trước hoàn tất hoặc không còn hợp lệ; không phát dispatch trùng.
                    return;
                }
            }
            catch (NotificationApplicationException)
            {
                // Lỗi nghiệp vụ/đọc Student đã được chuẩn hóa: lưu trạng thái snapshot failed để API có thể quan sát.
                // Các exception khác được ném lại để MassTransit áp retry policy của queue.
                await repository.MarkSnapshotFailedAsync(command.BatchId, cancellationToken);
                return;
            }
        }

        if (work.ShouldDispatch || work.ShouldReadRecipients)
        {
            // Mỗi command chỉ là một token kích hoạt. Dispatch handler sẽ dùng lease để claim chunk riêng,
            // nên nhiều token có thể xử lý song song mà không gửi cùng một recipient hai lần.
            for (var index = 0; index < options.DispatchChunkConcurrency; index++)
            {
                await commandSender.SendAsync(
                    ServiceNames.Notification,
                    new DispatchNotificationBatchV1(command.BatchId),
                    cancellationToken);
            }
        }
    }
}
