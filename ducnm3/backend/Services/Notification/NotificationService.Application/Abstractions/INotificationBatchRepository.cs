namespace NotificationService.Application.Abstractions;

public interface INotificationBatchRepository
{
    Task<NotificationBatchSummary> CreateAsync(CreateNotificationBatchRecord record, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<NotificationSnapshotWork> PrepareSnapshotAsync(Guid batchId, CancellationToken cancellationToken);
    Task AppendSnapshotPageAsync(Guid batchId, IReadOnlyList<Guid> studentIds, CancellationToken cancellationToken);
    Task<bool> CompleteSnapshotAsync(Guid batchId, CancellationToken cancellationToken);
    Task MarkSnapshotFailedAsync(Guid batchId, CancellationToken cancellationToken);
    Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken);
    Task<NotificationBatchFailedItemsPage> GetFailedItemsAsync(Guid batchId, Guid? afterItemId, int limit, CancellationToken cancellationToken);
    Task<NotificationBatchClaim?> ClaimChunkAsync(Guid batchId, CancellationToken cancellationToken);
    Task<IReadOnlyList<NotificationSummary>> CompleteClaimAsync(
        NotificationBatchClaim claim,
        IReadOnlyList<NotificationBatchDeliveryResult> results,
        CancellationToken cancellationToken);
    Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken);
}

public sealed record CreateNotificationBatchRecord(
    Guid Id, string Title, string BodyMarkdown, Guid CreatedBy, uint BatchSize, DateTime CreatedAtUtc);

public sealed record NotificationSnapshotWork(bool ShouldReadRecipients, bool ShouldDispatch);

public sealed record NotificationBatchSummary(
    Guid Id, string Status, uint TotalCount, uint ProcessedCount, uint SuccessCount, uint FailedCount, uint BatchSize, DateTime CreatedAtUtc, DateTime? StartedAtUtc, DateTime? CompletedAtUtc);

public sealed record NotificationBatchFailedItem(Guid StudentId, uint RetryCount, string ErrorMessage);

public sealed record NotificationBatchFailedItemsPage(IReadOnlyList<NotificationBatchFailedItem> Items, Guid? NextItemId, bool HasNextPage);

public sealed record NotificationBatchWorkItem(
    Guid Id, Guid BatchId, Guid StudentId, uint RetryCount, string Title, string BodyMarkdown, Guid CreatedBy);

public sealed record NotificationBatchClaim(Guid BatchId, Guid LeaseToken, IReadOnlyList<NotificationBatchWorkItem> Items);

public sealed record NotificationBatchDeliveryResult(Guid ItemId, bool IsSuccess, string? ErrorMessage);
