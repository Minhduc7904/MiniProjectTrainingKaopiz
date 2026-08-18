// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchSummary.cs
// Mục đích: Biểu diễn trạng thái và counter Notification Batch trả cho API mà không phụ thuộc EF model.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchSummary(Guid Id, string Status, uint TotalCount,
    uint ProcessedCount, uint SuccessCount, uint FailedCount, uint BatchSize,
    DateTime CreatedAtUtc, DateTime? StartedAtUtc, DateTime? CompletedAtUtc);
