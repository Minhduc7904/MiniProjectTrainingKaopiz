namespace MediaService.Api.Contracts.Responses;

public sealed record CreateMediaUsageResponse(
    Guid Id,
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    DateTime CreatedAtUtc);
