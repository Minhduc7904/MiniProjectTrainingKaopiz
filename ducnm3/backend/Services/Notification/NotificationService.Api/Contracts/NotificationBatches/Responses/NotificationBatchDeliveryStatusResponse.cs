// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Responses/NotificationBatchDeliveryStatusResponse.cs
// Mục đích: Khai báo HTTP response riêng cho counter và thời gian của bước delivery Notification Batch.

namespace NotificationService.Api.Contracts.NotificationBatches.Responses;

public sealed record NotificationBatchDeliveryStatusResponse(
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
