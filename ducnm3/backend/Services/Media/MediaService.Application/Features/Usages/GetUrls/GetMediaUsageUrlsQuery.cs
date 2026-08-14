namespace MediaService.Application.Features.Usages.GetUrls;

public sealed record GetMediaUsageUrlsQuery(
    string OwnerService,
    string OwnerType,
    string UsageType,
    Guid OwnerId);
