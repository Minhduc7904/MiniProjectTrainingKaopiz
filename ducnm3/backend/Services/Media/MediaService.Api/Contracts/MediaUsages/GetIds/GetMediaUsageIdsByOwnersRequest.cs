namespace MediaService.Api.Contracts.MediaUsages.GetIds;

public sealed record GetMediaUsageIdsByOwnersRequest(
    IReadOnlyList<MediaUsageOwnerScopeRequest>? Owners);

public sealed record MediaUsageOwnerScopeRequest(
    string? OwnerService,
    string? OwnerType,
    string? OwnerId);
