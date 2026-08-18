// File: backend/Services/Notification/NotificationService.Api/Contracts/NotificationBatches/Responses/NotificationBatchFailedItemsResponse.cs
// Mục đích: Trả các recipient gửi thất bại, số lần retry và cursor phân trang của một Notification Batch.

namespace NotificationService.Api.Contracts.NotificationBatches.Responses;

public sealed record NotificationBatchFailedItemsResponse(
    IReadOnlyList<NotificationBatchFailedItemResponse> Items);

public sealed record NotificationBatchFailedItemResponse(
    Guid StudentId,
    uint RetryCount,
    string ErrorMessage);
