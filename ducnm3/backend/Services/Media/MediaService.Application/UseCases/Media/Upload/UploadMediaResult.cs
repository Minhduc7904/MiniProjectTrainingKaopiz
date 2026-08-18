// File: backend/Services/Media/MediaService.Application/UseCases/Media/Upload/UploadMediaResult.cs
// Mục đích: Định nghĩa dữ liệu đầu ra của use case UploadMediaResult.

namespace MediaService.Application.UseCases.Media.Upload;

public sealed record UploadMediaResult(
    Guid Id,
    string MediaType,
    string ContentType,
    long SizeBytes,
    string Status,
    bool IsDraft,
    DateTime? DraftedAtUtc,
    string ThumbnailStatus = "NOT_REQUIRED",
    Guid? ThumbnailMediaId = null,
    Guid? ThumbnailJobId = null);
