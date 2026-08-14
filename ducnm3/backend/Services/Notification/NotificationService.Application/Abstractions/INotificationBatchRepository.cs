namespace NotificationService.Application.Abstractions;

public interface INotificationBatchRepository
{
    Task<NotificationBatchSummary> CreateAsync(CreateNotificationBatchRecord record, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken);
    Task<IReadOnlyList<NotificationBatchWorkItem>> ClaimChunkAsync(Guid batchId, CancellationToken cancellationToken);
    Task MarkSuccessAsync(NotificationBatchWorkItem item, CancellationToken cancellationToken);
    Task MarkFailureAsync(NotificationBatchWorkItem item, string errorMessage, CancellationToken cancellationToken);
    Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken);
}

public sealed record CreateNotificationBatchRecord(
    Guid Id, string Title, string BodyMarkdown, Guid CreatedBy, uint BatchSize, IReadOnlyList<Guid> StudentIds, DateTime CreatedAtUtc);

public sealed record NotificationBatchSummary(
    Guid Id, string Status, uint TotalCount, uint ProcessedCount, uint SuccessCount, uint FailedCount, uint BatchSize, DateTime CreatedAtUtc, DateTime? StartedAtUtc, DateTime? CompletedAtUtc);

public sealed record NotificationBatchWorkItem(
    Guid Id, Guid BatchId, Guid StudentId, uint RetryCount, string Title, string BodyMarkdown, Guid CreatedBy);
