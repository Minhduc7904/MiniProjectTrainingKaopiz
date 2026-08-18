// File: backend/Services/Notification/NotificationService.Api/Contracts/Notifications/Responses/NotificationResponse.cs
// Mục đích: Trả nội dung, nguồn, trạng thái đọc và các mốc thời gian của một Notification qua HTTP.

namespace NotificationService.Api.Contracts.Notifications.Responses;

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
