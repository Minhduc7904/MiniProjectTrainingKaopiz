namespace NotificationService.Api.Contracts.Requests;

public sealed record CreateNotificationBatchRequest(
    string Title,
    string BodyMarkdown,
    string TargetScope,
    string CreatedBy,
    uint? BatchSize,
    string? CourseId);
