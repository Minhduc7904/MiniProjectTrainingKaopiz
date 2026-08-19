// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Requests/CreateNotificationBatchRequest.cs
// Mục đích: Nhận nội dung, actor, batch size và số recipient tùy chọn; null requestedCount nghĩa là gửi toàn bộ.

namespace NotificationService.Api.Contracts.NotificationBatches.Requests;

public sealed record CreateNotificationBatchRequest(
    string Title,
    string BodyMarkdown,
    string TargetScope,
    string CreatedBy,
    uint? BatchSize,
    uint? RequestedCount,
    string? CourseId);
