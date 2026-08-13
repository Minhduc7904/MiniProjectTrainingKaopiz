using MediaService.Domain.Actors;

namespace MediaService.Application.Features.Usages.Create;

public sealed record CreateMediaUsageCommand(
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    ActorReference CreatedBy);
