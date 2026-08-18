// File: backend/Services/Notification/NotificationService.Application/Repositories/INotificationBatchRepository.cs
// Mục đích: Định nghĩa các thao tác tạo/query batch và lưu snapshot recipient cho Create, Get và Snapshot use case.

namespace NotificationService.Application.Repositories;

using NotificationService.Application.Repositories.Models;

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
}
