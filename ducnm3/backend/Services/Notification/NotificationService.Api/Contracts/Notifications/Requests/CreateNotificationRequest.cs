// File: backend/Services/Notification/NotificationService.Api/Contracts/Notifications/Requests/CreateNotificationRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho CreateNotificationRequest.

namespace NotificationService.Api.Contracts.Requests;

public sealed record CreateNotificationRequest(
    string StudentId,
    string Title,
    string BodyMarkdown,
    string CreatedBy);
