using MediaService.Domain.Actors;

namespace MediaService.Application.Abstractions.Persistence;

public sealed record CreateMediaUsageRecord(
    Guid Id,
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    ActorReference CreatedBy);
