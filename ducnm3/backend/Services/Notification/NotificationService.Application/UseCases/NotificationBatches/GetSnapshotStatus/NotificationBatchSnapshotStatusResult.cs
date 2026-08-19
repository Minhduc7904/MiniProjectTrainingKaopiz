// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/GetSnapshotStatus/NotificationBatchSnapshotStatusResult.cs
// Mục đích: Định nghĩa kết quả công khai của use case đọc tiến độ snapshot recipient.

namespace NotificationService.Application.UseCases.NotificationBatches.GetSnapshotStatus;

public sealed record NotificationBatchSnapshotStatusResult(
    Guid BatchId,
    string Status,
    uint SnapshotCount,
    uint? TargetCount,
    decimal? ProgressPercent);
