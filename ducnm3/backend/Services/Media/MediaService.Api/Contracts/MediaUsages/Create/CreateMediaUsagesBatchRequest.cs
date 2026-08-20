namespace MediaService.Api.Contracts.Requests;

public sealed record CreateMediaUsagesBatchRequest(
    IReadOnlyList<CreateMediaUsageRequest> Items);
