namespace MediaService.Application.Features.Usages.Create;

public sealed record CreateMediaUsageResult(
    Guid Id,
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    DateTime CreatedAtUtc);
