// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchSnapshotProgress.cs
// Mục đích: Mang projection chỉ đọc của bước snapshot từ Persistence sang Application mà không làm lộ EF entity.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchSnapshotProgress(
    Guid BatchId,
    string BatchStatus,
    uint? RequestedCount,
    uint SnapshotCount,
    uint? TotalCount);
