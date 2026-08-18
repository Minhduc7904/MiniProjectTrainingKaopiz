// File: backend/Services/Notification/NotificationService.Application/Services/Sending/INotificationSender.cs
// Mục đích: Khai báo hoặc triển khai adapter gửi notification cho INotificationSender.

namespace NotificationService.Application.Abstractions;

public interface INotificationSender
{
    Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken);
}
