// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/GetDeliveryStatus/NotificationBatchDeliveryStatusResult.cs
// Mục đích: Định nghĩa counter, phần trăm và thời gian của riêng bước delivery Notification Batch.

namespace NotificationService.Application.UseCases.NotificationBatches.GetDeliveryStatus;

public sealed record NotificationBatchDeliveryStatusResult(
    Guid BatchId,
    string Status,
    uint TotalCount,
    uint ProcessedCount,
    uint SuccessCount,
    uint FailedCount,
    uint RemainingCount,
    decimal? ProgressPercent,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    long? DurationMs);
