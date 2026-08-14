namespace NotificationService.Api.Contracts.Requests;

public sealed record CreateNotificationRequest(
    string StudentId,
    string Title,
    string BodyMarkdown,
    string CreatedBy);
