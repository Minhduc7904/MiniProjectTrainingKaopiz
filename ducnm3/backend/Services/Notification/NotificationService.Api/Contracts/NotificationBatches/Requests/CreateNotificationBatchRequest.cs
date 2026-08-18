// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Requests/CreateNotificationBatchRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho CreateNotificationBatchRequest.

namespace NotificationService.Api.Contracts.Requests;

public sealed record CreateNotificationBatchRequest(
    string Title,
    string BodyMarkdown,
    string TargetScope,
    string CreatedBy,
    uint? BatchSize,
    string? CourseId);
