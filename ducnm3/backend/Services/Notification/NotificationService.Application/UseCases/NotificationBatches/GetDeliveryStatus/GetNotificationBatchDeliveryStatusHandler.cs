// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/GetDeliveryStatus/GetNotificationBatchDeliveryStatusHandler.cs
// Mục đích: Trả trạng thái delivery từ counter batch và clock hiện tại mà không phát sinh write hay message.

using NotificationService.Application.Common.Errors;
using NotificationService.Application.Repositories;
using NotificationService.Domain.Constants;

namespace NotificationService.Application.UseCases.NotificationBatches.GetDeliveryStatus;

public sealed class GetNotificationBatchDeliveryStatusHandler(
    INotificationBatchRepository repository,
    TimeProvider timeProvider)
{
    public async Task<NotificationBatchDeliveryStatusResult> HandleAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        if (batchId == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId must be a valid UUID.");
        }

        var batch = await repository.GetByIdAsync(batchId, cancellationToken)
            ?? throw NotificationErrors.BatchNotFound();
        batch = NotificationBatchDuration.Calculate(batch, timeProvider.GetUtcNow().UtcDateTime);
        var status = batch.Status switch
        {
            NotificationBatchStatuses.Pending or NotificationBatchStatuses.Snapshotting or
                NotificationBatchStatuses.SnapshotReady => NotificationBatchStepStatuses.Pending,
            NotificationBatchStatuses.Processing => NotificationBatchStepStatuses.Running,
            NotificationBatchStatuses.Completed => NotificationBatchStepStatuses.Completed,
            _ => NotificationBatchStepStatuses.Failed,
        };
        decimal? percent = batch.TotalCount > 0
            ? Math.Min(100m, Math.Round(batch.ProcessedCount * 100m / batch.TotalCount, 2))
            : status is NotificationBatchStepStatuses.Completed or NotificationBatchStepStatuses.Failed
                ? 100m
                : null;

        return new NotificationBatchDeliveryStatusResult(
            batch.Id,
            status,
            batch.TotalCount,
            batch.ProcessedCount,
            batch.SuccessCount,
            batch.FailedCount,
            batch.TotalCount > batch.ProcessedCount ? batch.TotalCount - batch.ProcessedCount : 0,
            percent,
            batch.StartedAtUtc,
            batch.CompletedAtUtc,
            batch.DurationMs);
    }
}
