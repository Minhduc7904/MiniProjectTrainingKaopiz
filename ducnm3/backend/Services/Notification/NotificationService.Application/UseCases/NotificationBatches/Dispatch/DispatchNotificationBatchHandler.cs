// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Dispatch/DispatchNotificationBatchHandler.cs
// Mục đích: Claim một chunk có lease, gọi sender từng recipient, hoàn tất kết quả và phát command cho phần việc còn lại.

using System.Collections.Concurrent;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Contracts.Messaging;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Services.Content;
using NotificationService.Application.Services.Sending;
using NotificationService.Application.UseCases.NotificationBatches;

namespace NotificationService.Application.UseCases.NotificationBatches.Dispatch;

public sealed class DispatchNotificationBatchHandler(
    INotificationBatchDispatchRepository repository,
    INotificationSender sender,
    ICommandSender commandSender,
    NotificationMediaReferenceExtractor mediaReferenceExtractor,
    NotificationBatchProcessingOptions options)
{
    public async Task HandleAsync(DispatchNotificationBatchV1 command, CancellationToken cancellationToken)
    {
        // Claim atomically một nhóm PENDING (hoặc lease hết hạn) để các worker/token dispatch khác
        // không gửi trùng recipient. Claim mang lease để item có thể được lấy lại nếu worker chết giữa chừng.
        var claim = await repository.ClaimChunkAsync(command.BatchId, cancellationToken);
        if (claim is null || claim.Items.Count == 0)
        {
            // Không có item có thể claim: batch có thể đã xong hoặc một token khác đang giữ lease.
            return;
        }

        // ConcurrentBag an toàn khi các tác vụ gửi song song cùng thêm kết quả delivery.
        var results = new ConcurrentBag<NotificationBatchDeliveryResult>();
        await Parallel.ForEachAsync(
            claim.Items,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                // Giới hạn gửi song song để không làm quá tải sender/provider; khác với số token dispatch.
                MaxDegreeOfParallelism = options.MaxConcurrentSends,
            },
            async (item, token) =>
            {
                try
                {
                    // RetryCount là số lần item đã thất bại trước đó; sender nhận attempt theo base-1.
                    await sender.SendAsync(item.StudentId, checked((int)item.RetryCount) + 1, token);
                    results.Add(new NotificationBatchDeliveryResult(item.Id, true, null));
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    // Không nuốt OperationCanceledException để worker shutdown/retry theo transport đúng cách.
                    // Lỗi gửi thông thường được ghi riêng cho item, vì các item còn lại vẫn nên tiếp tục xử lý.
                    results.Add(new NotificationBatchDeliveryResult(item.Id, false, "Fake sender failed."));
                }
            });

        // Lưu kết quả, giải phóng lease và tạo notification records cho những item gửi thành công.
        var successfulNotifications = await repository.CompleteClaimAsync(
            claim,
            results.ToArray(),
            cancellationToken);

        if (successfulNotifications.Count > 0)
        {
            // Media Service chỉ cần được báo khi notification đã tạo thành công. Body của batch giống nhau
            // cho mọi recipient, nên có thể đọc media reference từ notification đầu tiên.
            var references = mediaReferenceExtractor.Extract(
                successfulNotifications[0].BodyMarkdown);
            if (references.Count > 0)
            {
                // Giới hạn đồng thời số notification id và số usage row của một message để không vượt contract Media.
                var notificationIdsPerCommand = Math.Min(
                    NotificationMediaUsageBatchLimits.MaxNotificationIdsPerCommand,
                    Math.Max(
                        1,
                        NotificationMediaUsageBatchLimits.MaxUsageRowsPerCommand /
                        references.Count));
                foreach (var notificationIds in successfulNotifications
                    .Select(notification => notification.Id)
                    .Distinct()
                    .Chunk(notificationIdsPerCommand))
                {
                    // Gửi từng lô usage sang Media Service; outbox phía consumer bảo đảm command chỉ rời DB sau commit.
                    await commandSender.SendAsync(
                        ServiceNames.Media,
                        new RegisterNotificationMediaUsageBatchV1(
                            command.BatchId,
                            notificationIds,
                            successfulNotifications[0].CreatedBy,
                            references),
                        cancellationToken);
                }
            }
        }

        var continuation = await repository.FinalizeOrHasRemainingAsync(
            command.BatchId,
            cancellationToken);
        if (continuation.ShouldDispatch)
        {
            // Tự enqueue token kế tiếp để lấy chunk còn lại. Không loop trực tiếp nhằm nhường queue cho worker khác
            // và giữ thời gian xử lý một message có giới hạn.
            await commandSender.SendAsync(ServiceNames.Notification, command, cancellationToken);
        }
        else if (continuation.IsTerminal)
        {
            // Chỉ batch terminal mới đóng Media Usage job. expectedUsageCount là số notification thành công
            // nhân số media reference, để Media Service biết đã nhận đủ usage hay chưa.
            var referenceCount = mediaReferenceExtractor.Extract(continuation.BodyMarkdown).Count;
            var expectedUsageCount = checked(continuation.SuccessCount * (uint)referenceCount);
            await commandSender.SendAsync(
                ServiceNames.Media,
                new CompleteNotificationMediaUsageJobV1(command.BatchId, expectedUsageCount),
                cancellationToken);
        }
    }
}
