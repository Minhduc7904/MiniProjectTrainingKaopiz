namespace NotificationService.Application.Abstractions;

public interface INotificationSender
{
    Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken);
}
