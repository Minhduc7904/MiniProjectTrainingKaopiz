// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchFailedItem.cs
// Mục đích: Biểu diễn một recipient gửi thất bại cùng retry count và thông báo lỗi an toàn.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchFailedItem(Guid StudentId, uint RetryCount, string ErrorMessage);
