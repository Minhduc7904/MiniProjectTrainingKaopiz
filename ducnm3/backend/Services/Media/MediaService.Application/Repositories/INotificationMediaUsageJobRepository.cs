// File: backend/Services/Media/MediaService.Application/Repositories/INotificationMediaUsageJobRepository.cs
// Mục đích: Định nghĩa port tạo, cập nhật nguyên tử và đọc status job Media Usage của Notification Batch.

namespace MediaService.Application.Repositories;

public interface INotificationMediaUsageJobRepository
{
    Task<NotificationMediaUsageJobRecord?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken);
    Task StartAsync(Guid jobId, CancellationToken cancellationToken);
    Task RecordSuccessAsync(Guid jobId, uint usageCount, CancellationToken cancellationToken);
    Task RecordFailureAsync(Guid jobId, uint usageCount, string safeError, CancellationToken cancellationToken);
    Task CompleteSourceAsync(Guid jobId, uint expectedUsageCount, CancellationToken cancellationToken);
}
