// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Responses/NotificationBatchSnapshotStatusResponse.cs
// Mục đích: Khai báo HTTP response riêng cho tiến độ snapshot recipient của Notification Batch.

namespace NotificationService.Api.Contracts.NotificationBatches.Responses;

public sealed record NotificationBatchSnapshotStatusResponse(
    Guid BatchId,
    string Status,
    uint SnapshotCount,
    uint? TargetCount,
    decimal? ProgressPercent);
