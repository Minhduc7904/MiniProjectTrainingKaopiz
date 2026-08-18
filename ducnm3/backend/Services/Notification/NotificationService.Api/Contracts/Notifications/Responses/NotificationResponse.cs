// File: backend/Services/Notification/NotificationService.Api/Contracts/Notifications/Responses/NotificationResponse.cs
// Mục đích: Định nghĩa response contract HTTP cho NotificationResponse.

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
