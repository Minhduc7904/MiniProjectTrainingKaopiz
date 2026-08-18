// File: backend/Services/Notification/NotificationService.Infrastructure/Services/Sending/FakeNotificationSender.cs
// Mục đích: Khai báo hoặc triển khai adapter gửi notification cho FakeNotificationSender.

using NotificationService.Application.Abstractions;

namespace NotificationService.Infrastructure.Sending;

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
