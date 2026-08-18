// File: backend/Services/Media/MediaService.Api/Contracts/MediaDerivations/GetThumbnailStatus/MediaThumbnailStatusResponse.cs
// Mục đích: Định nghĩa response contract HTTP cho MediaThumbnailStatusResponse.

namespace MediaService.Api.Contracts.Responses;

public sealed record MediaThumbnailStatusResponse(
    Guid SourceMediaId,
    Guid JobId,
    string Status,
    Guid ThumbnailMediaId,
    Guid? ActiveThumbnailMediaId,
    string? ThumbnailContentUrl,
    string? LastError,
    DateTime UpdatedAtUtc);
