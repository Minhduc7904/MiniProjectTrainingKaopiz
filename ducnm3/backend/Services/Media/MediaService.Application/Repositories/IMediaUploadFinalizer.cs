// File: backend/Services/Media/MediaService.Application/Repositories/IMediaUploadFinalizer.cs
// Mục đích: Định nghĩa port hoàn tất upload trực tiếp một cách nguyên tử, gồm chuyển draft sang ready và tạo các tác vụ liên quan.

using MediaService.Application.Services.Storage;
using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.Repositories;

public interface IMediaUploadFinalizer
{
    Task<MediaUploadFinalizationResult> FinalizeAsync(
        MediaUploadFinalizationRequest request,
        CancellationToken cancellationToken);
}

public sealed record MediaUploadFinalizationRequest(
    Guid SourceMediaId,
    string SourceMediaType,
    string SourceContentType,
    string SourceChecksumSha256,
    DateTime CompletedAtUtc,
    ThumbnailReservation? Thumbnail,
    ActorReference? RequestedBy = null,
    StorageObjectLocation? FinalLocation = null);

public sealed record ThumbnailReservation(
    Guid JobId,
    Guid MediaId,
    StorageObjectLocation Location,
    string OriginalFileName,
    ActorReference UploadedBy);

public sealed record MediaUploadFinalizationResult(
    DateTime CompletedAtUtc,
    string ThumbnailStatus,
    Guid? ThumbnailMediaId,
    Guid? ThumbnailJobId,
    bool Transitioned = true);
