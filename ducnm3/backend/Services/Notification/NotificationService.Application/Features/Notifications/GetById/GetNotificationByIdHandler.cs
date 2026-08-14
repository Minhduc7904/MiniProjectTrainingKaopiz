using NotificationService.Application.Abstractions;

namespace NotificationService.Application.Features.Notifications.GetById;

public sealed class GetNotificationByIdHandler(INotificationRepository repository)
{
    public async Task<NotificationSummary> HandleAsync(
        Guid notificationId,
        CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(notificationId, cancellationToken) ??
        throw NotificationErrors.NotificationNotFound();
}
