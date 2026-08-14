namespace NotificationService.Api.Contracts.Responses;

public sealed record NotificationResponse(
    Guid Id,
    Guid RecipientStudentId,
    string Title,
    string BodyMarkdown,
    string SourceType,
    string Status,
    Guid CreatedBy,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
