// File: backend/Services/Notification/NotificationService.Infrastructure/Services/Sending/SuccessfulNotificationSender.cs
// Mục đích: Hoàn tất delivery Notification nội bộ mặc định để sau này thay bằng email hoặc SMS provider.

using NotificationService.Application.Services.Sending;

namespace NotificationService.Infrastructure.Services.Sending;

public sealed class SuccessfulNotificationSender : INotificationSender
{
    public Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
