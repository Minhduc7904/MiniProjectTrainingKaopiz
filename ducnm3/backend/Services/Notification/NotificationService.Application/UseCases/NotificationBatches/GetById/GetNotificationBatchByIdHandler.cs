// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/GetById/GetNotificationBatchByIdHandler.cs
// Mục đích: Điều phối use case GetNotificationBatchByIdHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using NotificationService.Application.Abstractions;

namespace NotificationService.Application.Features.Batches.GetById;

public sealed class GetNotificationBatchByIdHandler(INotificationBatchRepository repository)
{
    public async Task<NotificationBatchSummary> HandleAsync(Guid batchId, CancellationToken cancellationToken)
    {
        if (batchId == Guid.Empty) throw NotificationErrors.Validation("batchId must be a valid UUID.");
        return await repository.GetByIdAsync(batchId, cancellationToken) ?? throw NotificationErrors.BatchNotFound();
    }
}
