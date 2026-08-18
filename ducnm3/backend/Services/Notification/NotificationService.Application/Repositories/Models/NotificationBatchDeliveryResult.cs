// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchDeliveryResult.cs
// Mục đích: Mang kết quả gửi từng batch item về persistence để áp dụng success, retry hoặc failed.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchDeliveryResult(Guid ItemId, bool IsSuccess, string? ErrorMessage);
