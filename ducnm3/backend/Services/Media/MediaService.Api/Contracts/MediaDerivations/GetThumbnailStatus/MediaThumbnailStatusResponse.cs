// File: backend/Services/Media/MediaService.Api/Contracts/MediaDerivations/GetThumbnailStatus/MediaThumbnailStatusResponse.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

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
