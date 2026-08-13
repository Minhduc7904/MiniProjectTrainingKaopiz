namespace MediaService.Api.Contracts.Requests;

public sealed record CreateMediaUsageRequest(
    string MediaId,
    string OwnerService,
    string OwnerType,
    string OwnerId,
    string UsageType,
    uint DisplayOrder,
    string CreatedByType,
    string CreatedBy);
