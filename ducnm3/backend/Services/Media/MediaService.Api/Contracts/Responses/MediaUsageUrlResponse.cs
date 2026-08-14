namespace MediaService.Api.Contracts.Responses;

public sealed record MediaUsageUrlResponse(
    Guid UsageId,
    Guid MediaId,
    string Url,
    DateTime? ExpiresAtUtc,
    uint DisplayOrder);
