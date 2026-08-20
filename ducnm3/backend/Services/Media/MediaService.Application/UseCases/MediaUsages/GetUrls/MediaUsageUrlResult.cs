// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrls/MediaUsageUrlResult.cs
// Mục đích: Định nghĩa dữ liệu đầu ra của use case MediaUsageUrlResult.

namespace MediaService.Application.UseCases.MediaUsages.GetUrls;

public sealed record MediaUsageUrlResult(
    Guid UsageId,
    Guid MediaId,
    Guid OwnerId,
    string ContentUrl,
    string? ThumbnailUrl,
    DateTime? ExpiresAtUtc,
    uint DisplayOrder);
