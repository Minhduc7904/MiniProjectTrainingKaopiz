// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Requests/CreateNotificationBatchRequest.cs
// Mục đích: Nhận title, Markdown, target scope, actor, batch size và idempotency key khi client tạo Notification Batch.

namespace NotificationService.Api.Contracts.NotificationBatches.Requests;

public sealed record CreateNotificationBatchRequest(
    string Title,
    string BodyMarkdown,
    string TargetScope,
    string CreatedBy,
    uint? BatchSize,
    string? CourseId);
