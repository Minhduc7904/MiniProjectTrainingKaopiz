// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Requests/CreateNotificationBatchRequest.cs
// Mục đích: Nhận nội dung, batch size và số recipient tùy chọn; actor được đọc từ header gateway.

namespace NotificationService.Api.Contracts.NotificationBatches.Requests;

public sealed record CreateNotificationBatchRequest(
    string Title,
    string BodyMarkdown,
    string TargetScope,
    uint? BatchSize,
    uint? RequestedCount,
    string? CourseId);
