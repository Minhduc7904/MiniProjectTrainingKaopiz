// File: backend/Services/Notification/NotificationService.Application/Repositories/INotificationRepository.cs
// Mục đích: Định nghĩa thao tác lưu Notification trực tiếp và đọc summary theo ID mà Application không phụ thuộc EF Core.

namespace NotificationService.Application.Repositories;

using NotificationService.Application.Repositories.Models;

public interface INotificationRepository
{
    Task<NotificationSummary> CreateAsync(
        CreateNotificationRecord record,
        CancellationToken cancellationToken);

    Task<NotificationSummary?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
