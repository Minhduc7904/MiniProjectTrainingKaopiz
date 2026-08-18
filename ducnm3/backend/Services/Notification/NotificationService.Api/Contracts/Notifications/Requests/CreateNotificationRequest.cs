// File: backend/Services/Notification/NotificationService.Api/Contracts/Notifications/Requests/CreateNotificationRequest.cs
// Mục đích: Nhận recipient, title, Markdown, source type và actor khi client tạo một Notification trực tiếp.

namespace NotificationService.Api.Contracts.Notifications.Requests;

public sealed record CreateNotificationRequest(
    string StudentId,
    string Title,
    string BodyMarkdown,
    string CreatedBy);
