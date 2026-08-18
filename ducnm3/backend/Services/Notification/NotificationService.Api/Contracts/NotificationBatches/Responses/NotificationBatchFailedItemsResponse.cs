// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Responses/NotificationBatchFailedItemsResponse.cs
// Mục đích: Định nghĩa response contract HTTP cho NotificationBatchFailedItemsResponse.

namespace NotificationService.Api.Contracts.Responses;

public sealed record NotificationBatchFailedItemsResponse(
    IReadOnlyList<NotificationBatchFailedItemResponse> Items);

public sealed record NotificationBatchFailedItemResponse(
    Guid StudentId,
    uint RetryCount,
    string ErrorMessage);
