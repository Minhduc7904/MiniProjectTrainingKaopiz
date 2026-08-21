namespace MediaService.Application.UseCases.MediaUsages.GetIds;

public sealed record GetMediaUsageIdsByOwnersQuery(
    IReadOnlyList<MediaUsageOwnerScopeQuery> Owners);

public sealed record MediaUsageOwnerScopeQuery(
    string OwnerService,
    string OwnerType,
    Guid OwnerId);
