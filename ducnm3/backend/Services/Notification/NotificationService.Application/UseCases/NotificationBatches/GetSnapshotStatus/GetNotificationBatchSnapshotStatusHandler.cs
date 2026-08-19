// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/GetSnapshotStatus/GetNotificationBatchSnapshotStatusHandler.cs
// Mục đích: Đọc snapshot count và chuyển trạng thái batch thành trạng thái riêng của bước snapshot cho UI polling.

using NotificationService.Application.Common.Errors;
using NotificationService.Application.Repositories;
using NotificationService.Domain.Constants;

namespace NotificationService.Application.UseCases.NotificationBatches.GetSnapshotStatus;

public sealed class GetNotificationBatchSnapshotStatusHandler(INotificationBatchRepository repository)
{
    public async Task<NotificationBatchSnapshotStatusResult> HandleAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        if (batchId == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId must be a valid UUID.");
        }

        var progress = await repository.GetSnapshotProgressAsync(batchId, cancellationToken)
            ?? throw NotificationErrors.BatchNotFound();
        var status = MapStatus(progress.BatchStatus, progress.TotalCount);
        var targetCount = status == NotificationBatchStepStatuses.Completed
            ? progress.TotalCount
            : progress.RequestedCount ?? progress.TotalCount;
        decimal? percent = targetCount is > 0
            ? Math.Min(100m, Math.Round(progress.SnapshotCount * 100m / targetCount.Value, 2))
            : status == NotificationBatchStepStatuses.Completed ? 100m : null;

        return new NotificationBatchSnapshotStatusResult(
            progress.BatchId,
            status,
            progress.SnapshotCount,
            targetCount,
            percent);
    }

    private static string MapStatus(string batchStatus, uint? totalCount) => batchStatus switch
    {
        NotificationBatchStatuses.Pending => NotificationBatchStepStatuses.Pending,
        NotificationBatchStatuses.Snapshotting => NotificationBatchStepStatuses.Running,
        NotificationBatchStatuses.Failed when totalCount is null or 0 => NotificationBatchStepStatuses.Failed,
        _ => NotificationBatchStepStatuses.Completed,
    };
}
