namespace NotificationService.Application.Features.Batches.Create;

public sealed record CreateNotificationBatchCommand(string Title, string BodyMarkdown, string TargetScope, Guid CreatedBy, uint? BatchSize, Guid? CourseId);
