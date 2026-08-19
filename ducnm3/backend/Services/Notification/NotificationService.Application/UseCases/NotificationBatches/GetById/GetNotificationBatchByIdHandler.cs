// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/GetById/GetNotificationBatchByIdHandler.cs
// Mục đích: Đọc summary Notification Batch theo ID và trả lỗi not-found chuẩn khi batch không tồn tại.

using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Common.Errors;

namespace NotificationService.Application.UseCases.NotificationBatches.GetById;

public sealed class GetNotificationBatchByIdHandler(
    INotificationBatchRepository repository,
    TimeProvider timeProvider)
{
    public async Task<NotificationBatchSummary> HandleAsync(Guid batchId, CancellationToken cancellationToken)
    {
        if (batchId == Guid.Empty) throw NotificationErrors.Validation("batchId must be a valid UUID.");
        var result = await repository.GetByIdAsync(batchId, cancellationToken)
            ?? throw NotificationErrors.BatchNotFound();
        return NotificationBatchDuration.Calculate(result, timeProvider.GetUtcNow().UtcDateTime);
    }
}
