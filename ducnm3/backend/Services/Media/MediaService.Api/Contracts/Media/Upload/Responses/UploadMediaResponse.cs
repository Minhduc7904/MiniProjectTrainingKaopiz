// File: backend/Services/Media/MediaService.Api/Contracts/Media/Upload/Responses/UploadMediaResponse.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

namespace MediaService.Api.Contracts.Responses;

public sealed record UploadMediaResponse(
    Guid Id,
    string MediaType,
    string ContentType,
    long SizeBytes,
    string Status,
    bool IsDraft,
    DateTime? DraftedAtUtc,
    string ContentUrl,
    string ThumbnailStatus,
    Guid? ThumbnailMediaId,
    Guid? ThumbnailJobId,
    string? ThumbnailStatusUrl);
