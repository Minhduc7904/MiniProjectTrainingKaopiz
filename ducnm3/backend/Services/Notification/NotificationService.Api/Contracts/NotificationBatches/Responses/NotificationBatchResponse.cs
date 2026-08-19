// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Responses/NotificationBatchResponse.cs
// Mục đích: Trả trạng thái, counter xử lý, batch size và các mốc thời gian của Notification Batch qua HTTP.

namespace NotificationService.Api.Contracts.NotificationBatches.Responses;

public sealed record NotificationBatchResponse(
    Guid Id,
    string Title,
    string Status,
    uint TotalCount,
    uint ProcessedCount,
    uint SuccessCount,
    uint FailedCount,
    uint BatchSize,
    uint? RequestedCount,
    Guid? SourceBatchId,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    long? DurationMs);
