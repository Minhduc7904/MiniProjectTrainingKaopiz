// File: backend/Services/Media/MediaService.Api/Contracts/MediaUsages/Create/CreateMediaUsageRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho CreateMediaUsageRequest.

namespace MediaService.Api.Contracts.Requests;

public sealed record CreateMediaUsageRequest(
    string MediaId,
    string OwnerService,
    string OwnerType,
    string OwnerId,
    string UsageType,
    uint DisplayOrder);
