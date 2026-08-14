namespace NotificationService.Application.Features.Notifications.Create;

public sealed record CreateNotificationCommand(
    Guid StudentId,
    string Title,
    string BodyMarkdown,
    Guid CreatedBy);
