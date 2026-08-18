// File: backend/Services/Notification/NotificationService.Domain/Entities/NotificationBatchItemState.cs
// Mục đích: Quyết định success, retry hoặc failed cho một Notification Batch Item theo hợp đồng retry hiện tại.

using NotificationService.Domain.Constants;

namespace NotificationService.Domain.Entities;

public sealed class NotificationBatchItemState(string status, uint retryCount)
{
    private const uint MaximumAttempts = 2;

    public string Status { get; private set; } = status;
    public uint RetryCount { get; private set; } = retryCount;
    public string? ErrorMessage { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    public void ApplyDeliveryResult(bool isSuccess, string? errorMessage, DateTime processedAtUtc)
    {
        if (isSuccess)
        {
            Status = NotificationBatchItemStatuses.Success;
            ErrorMessage = null;
            ProcessedAtUtc = processedAtUtc;
            return;
        }
        RetryCount++;
        Status = RetryCount >= MaximumAttempts
            ? NotificationBatchItemStatuses.Failed
            : NotificationBatchItemStatuses.Retry;
        ErrorMessage = errorMessage ?? "Notification sender failed.";
        ProcessedAtUtc = Status == NotificationBatchItemStatuses.Failed ? processedAtUtc : null;
    }
}
