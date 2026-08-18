// File: backend/Services/Media/MediaService.Api/Contracts/MediaUsages/Create/CreateMediaUsageRequest.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

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
