// File: backend/Services/Notification/NotificationService.Application/Services/Sending/INotificationSender.cs
// Mục đích: Định nghĩa port gửi Notification để Dispatch use case không phụ thuộc delivery provider cụ thể.

namespace NotificationService.Application.Services.Sending;

public interface INotificationSender
{
    Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken);
}
