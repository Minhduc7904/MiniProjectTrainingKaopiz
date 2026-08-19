// File: backend/Services/Notification/NotificationService.Application/Repositories/INotificationBatchRepository.cs
// Mục đích: Định nghĩa các thao tác tạo/query batch và lưu snapshot recipient cho Create, Get và Snapshot use case.

namespace NotificationService.Application.Repositories;

using NotificationService.Application.Repositories.Models;

public interface INotificationBatchRepository
{
    Task<NotificationBatchSummary> CreateAsync(CreateNotificationBatchRecord record, CancellationToken cancellationToken);
    Task<NotificationBatchRetryCreation> PrepareRetryAsync(Guid sourceBatchId, Guid createdBy, DateTime createdAtUtc, CancellationToken cancellationToken);
    Task<NotificationBatchSummary> CommitRetryAsync(Guid sourceBatchId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<NotificationSnapshotWork> PrepareSnapshotAsync(Guid batchId, CancellationToken cancellationToken);
    Task<int> AppendSnapshotPageAsync(Guid batchId, IReadOnlyList<Guid> studentIds, CancellationToken cancellationToken);
    Task CopyFailedRecipientsAsync(Guid batchId, Guid sourceBatchId, CancellationToken cancellationToken);
    Task<bool> CompleteSnapshotAsync(Guid batchId, CancellationToken cancellationToken);
    Task MarkSnapshotFailedAsync(Guid batchId, CancellationToken cancellationToken);
    Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken);
    Task<NotificationBatchSnapshotProgress?> GetSnapshotProgressAsync(Guid batchId, CancellationToken cancellationToken);
    Task<NotificationBatchListPage> ListAsync(string? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<NotificationBatchFailedItemsPage> GetFailedItemsAsync(Guid batchId, Guid? afterItemId, int limit, CancellationToken cancellationToken);
}
