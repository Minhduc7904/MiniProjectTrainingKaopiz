// File: backend/Services/Media/MediaService.Api/Contracts/MediaUsages/GetUrls/MediaUsageUrlResponse.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

namespace MediaService.Api.Contracts.Responses;

public sealed record MediaUsageUrlResponse(
    Guid UsageId,
    Guid MediaId,
    string Url,
    DateTime? ExpiresAtUtc,
    uint DisplayOrder);
