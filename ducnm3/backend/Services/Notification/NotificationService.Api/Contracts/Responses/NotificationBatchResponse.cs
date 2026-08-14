namespace NotificationService.Api.Contracts.Responses;

public sealed record NotificationBatchResponse(Guid Id, string Status, uint TotalCount, uint ProcessedCount, uint SuccessCount, uint FailedCount, uint BatchSize, DateTime CreatedAtUtc, DateTime? StartedAtUtc, DateTime? CompletedAtUtc);
