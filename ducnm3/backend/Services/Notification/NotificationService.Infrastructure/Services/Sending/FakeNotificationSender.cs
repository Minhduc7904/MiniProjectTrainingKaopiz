// File: backend/Services/Notification/NotificationService.Infrastructure/Services/Sending/FakeNotificationSender.cs
// Mục đích: Giả lập gửi Notification với quy tắc lỗi xác định theo student hash và attempt để kiểm thử retry cục bộ.

using NotificationService.Application.Services.Sending;

namespace NotificationService.Infrastructure.Services.Sending;

public sealed class FakeNotificationSender : INotificationSender
{
    public Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var value = unchecked((uint)studentId.GetHashCode());
        if ((attempt == 1 && value % 20 == 0) || (attempt == 2 && value % 100 == 0))
        {
            throw new InvalidOperationException("Fake notification sender failure.");
        }

        return Task.CompletedTask;
    }
}
