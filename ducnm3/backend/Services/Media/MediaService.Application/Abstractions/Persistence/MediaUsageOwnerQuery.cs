namespace MediaService.Application.Abstractions.Persistence;

public sealed record MediaUsageOwnerQuery(
    string OwnerService,
    string OwnerType,
    string UsageType,
    Guid OwnerId);
