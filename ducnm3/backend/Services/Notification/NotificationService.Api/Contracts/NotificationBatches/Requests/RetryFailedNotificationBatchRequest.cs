// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Requests/RetryFailedNotificationBatchRequest.cs
// Mục đích: Nhận actor tạo batch retry để audit đúng người yêu cầu gửi lại các recipient thất bại.

namespace NotificationService.Api.Contracts.NotificationBatches.Requests;

public sealed record RetryFailedNotificationBatchRequest(string CreatedBy);
