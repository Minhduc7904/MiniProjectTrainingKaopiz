// File: backend/Services/Notification/NotificationService.Domain/Entities/NotificationBatchState.cs
// Mục đích: Sở hữu transition trạng thái và counter của Notification Batch, độc lập EF Core và transport.

using NotificationService.Domain.Constants;

namespace NotificationService.Domain.Entities;

public enum NotificationSnapshotDecision
{
    None,
    ReadRecipients,
    Dispatch,
}

public sealed class NotificationBatchState(
    string status,
    uint totalCount,
    uint processedCount,
    uint successCount,
    uint failedCount,
    DateTime? startedAtUtc,
    DateTime? completedAtUtc)
{
    public string Status { get; private set; } = status;
    public uint TotalCount { get; private set; } = totalCount;
    public uint ProcessedCount { get; private set; } = processedCount;
    public uint SuccessCount { get; private set; } = successCount;
    public uint FailedCount { get; private set; } = failedCount;
    public DateTime? StartedAtUtc { get; private set; } = startedAtUtc;
    public DateTime? CompletedAtUtc { get; private set; } = completedAtUtc;

    public NotificationSnapshotDecision PrepareSnapshot()
    {
        if (Status is NotificationBatchStatuses.Completed or NotificationBatchStatuses.PartialFailed or NotificationBatchStatuses.Failed)
        {
            return NotificationSnapshotDecision.None;
        }
        if (Status == NotificationBatchStatuses.SnapshotReady)
        {
            return NotificationSnapshotDecision.Dispatch;
        }
        Status = NotificationBatchStatuses.Snapshotting;
        return NotificationSnapshotDecision.ReadRecipients;
    }

    public bool CompleteSnapshot(uint totalCount, DateTime completedAtUtc)
    {
        if (Status != NotificationBatchStatuses.Snapshotting)
        {
            return Status == NotificationBatchStatuses.SnapshotReady;
        }
        TotalCount = totalCount;
        if (totalCount == 0)
        {
            Status = NotificationBatchStatuses.Failed;
            CompletedAtUtc = completedAtUtc;
            return false;
        }
        Status = NotificationBatchStatuses.SnapshotReady;
        return true;
    }

    public void MarkSnapshotFailed(DateTime completedAtUtc)
    {
        if (Status is NotificationBatchStatuses.Completed or NotificationBatchStatuses.PartialFailed or NotificationBatchStatuses.Failed)
        {
            return;
        }
        Status = NotificationBatchStatuses.Failed;
        CompletedAtUtc = completedAtUtc;
    }

    public void StartProcessing(DateTime startedAtUtc)
    {
        Status = NotificationBatchStatuses.Processing;
        StartedAtUtc ??= startedAtUtc;
    }

    public void AddDeliveryCounts(uint successCount, uint failedCount)
    {
        SuccessCount = checked(SuccessCount + successCount);
        FailedCount = checked(FailedCount + failedCount);
        ProcessedCount = checked(ProcessedCount + successCount + failedCount);
    }

    public bool FinalizeOrHasRemaining(bool hasPending, bool hasActiveClaim, DateTime completedAtUtc)
    {
        if (hasPending)
        {
            return true;
        }
        if (hasActiveClaim)
        {
            return false;
        }
        Status = FailedCount == 0
            ? NotificationBatchStatuses.Completed
            : SuccessCount == 0
                ? NotificationBatchStatuses.Failed
                : NotificationBatchStatuses.PartialFailed;
        CompletedAtUtc = completedAtUtc;
        return false;
    }
}
