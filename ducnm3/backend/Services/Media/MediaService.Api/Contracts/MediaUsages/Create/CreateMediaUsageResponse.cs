// File: backend/Services/Media/MediaService.Api/Contracts/MediaUsages/Create/CreateMediaUsageResponse.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

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
