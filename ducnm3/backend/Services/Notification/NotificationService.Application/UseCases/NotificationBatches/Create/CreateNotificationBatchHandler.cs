// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Create/CreateNotificationBatchHandler.cs
// Mục đích: Validate batch ALL_STUDENTS, lưu trạng thái PENDING và phát command snapshot recipient qua outbox.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Contracts.Messaging;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Services.Content;
using NotificationService.Domain.Constants;

namespace NotificationService.Application.UseCases.NotificationBatches.Create;

public sealed class CreateNotificationBatchHandler(
    INotificationBatchRepository repository,
    ICommandSender commandSender,
    NotificationMediaReferenceExtractor mediaReferenceExtractor,
    TimeProvider timeProvider)
{
    public async Task<NotificationBatchSummary> HandleAsync(CreateNotificationBatchCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Batch hiện chỉ hỗ trợ gửi cho toàn bộ Student; chặn các field/scope ngoài contract trước khi tạo dữ liệu.
        if (!string.Equals(command.TargetScope?.Trim(), NotificationTargetScopes.AllStudents, StringComparison.Ordinal) || command.CourseId is not null || command.CreatedBy == Guid.Empty || string.IsNullOrWhiteSpace(command.Title) || command.Title.Length > 200 || string.IsNullOrWhiteSpace(command.BodyMarkdown))
        {
            throw NotificationErrors.Validation("Only ALL_STUDENTS, title and bodyMarkdown are accepted.");
        }

        // Áp dụng batch mặc định và giới hạn kích thước để Worker không nhận workload quá lớn trong một lượt.
        var batchSize = command.BatchSize ?? 500;
        if (batchSize is < 1 or > 1000)
        {
            throw NotificationErrors.Validation("batchSize must be between 1 and 1000.");
        }

        if (command.RequestedCount is 0 or > 100000)
        {
            throw NotificationErrors.Validation("requestedCount must be null or between 1 and 100000.");
        }

        // Parse/validate các Media reference trước khi ghi batch; kết quả sẽ được Media job xử lý sau đó.
        _ = mediaReferenceExtractor.Extract(command.BodyMarkdown);

        // Lưu batch ở trạng thái PENDING; snapshot recipient được thực hiện bất đồng bộ, không nằm trong HTTP request.
        var result = await repository.CreateAsync(new CreateNotificationBatchRecord(
            Guid.NewGuid(), command.Title.Trim(), command.BodyMarkdown,
            NotificationTargetScopes.AllStudents, command.CreatedBy, batchSize,
            command.RequestedCount, null, timeProvider.GetUtcNow().UtcDateTime), cancellationToken);
        // Queue snapshot recipient và đồng bộ Media usage để hai Worker tiếp tục xử lý theo cùng batch ID.
        await commandSender.SendAsync(ServiceNames.Notification, new SnapshotNotificationBatchV1(result.Id), cancellationToken);
        await commandSender.SendAsync(
            ServiceNames.Media,
            new StartNotificationMediaUsageJobV1(result.Id),
            cancellationToken);
        // Commit batch và các outbox command đã được đăng ký ở trên.
        await repository.SaveChangesAsync(cancellationToken);
        return result;
    }
}
