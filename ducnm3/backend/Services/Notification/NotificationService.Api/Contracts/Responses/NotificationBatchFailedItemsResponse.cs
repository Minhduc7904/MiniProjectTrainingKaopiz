namespace NotificationService.Api.Contracts.Responses;

public sealed record NotificationBatchFailedItemsResponse(
    IReadOnlyList<NotificationBatchFailedItemResponse> Items);

public sealed record NotificationBatchFailedItemResponse(
    Guid StudentId,
    uint RetryCount,
    string ErrorMessage);
