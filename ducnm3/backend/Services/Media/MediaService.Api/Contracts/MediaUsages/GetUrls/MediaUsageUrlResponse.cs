// File: backend/Services/Media/MediaService.Api/Contracts/MediaUsages/GetUrls/MediaUsageUrlResponse.cs
// Mục đích: Định nghĩa response contract HTTP cho MediaUsageUrlResponse.

namespace MediaService.Api.Contracts.Responses;

public sealed record MediaUsageUrlResponse(
    Guid UsageId,
    Guid MediaId,
    Guid OwnerId,
    string ContentUrl,
    string? ThumbnailUrl,
    DateTime? ExpiresAtUtc,
    uint DisplayOrder);
