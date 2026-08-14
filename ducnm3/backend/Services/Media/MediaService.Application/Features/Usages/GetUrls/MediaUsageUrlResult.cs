namespace MediaService.Application.Features.Usages.GetUrls;

public sealed record MediaUsageUrlResult(
    Guid UsageId,
    Guid MediaId,
    string Url,
    DateTime? ExpiresAtUtc,
    uint DisplayOrder);
