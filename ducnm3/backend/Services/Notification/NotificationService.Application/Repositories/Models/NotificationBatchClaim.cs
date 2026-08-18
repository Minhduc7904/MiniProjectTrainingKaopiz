// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchClaim.cs
// Mục đích: Đóng gói lease token và các item thuộc cùng một lần claim Notification Batch.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchClaim(
    Guid BatchId, Guid LeaseToken, IReadOnlyList<NotificationBatchWorkItem> Items);
