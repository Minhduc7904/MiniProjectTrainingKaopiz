using MediaService.Application.Abstractions.Storage;
using MediaService.Domain.Actors;

namespace MediaService.Application.Abstractions.Persistence;

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
    ThumbnailReservation? Thumbnail);

public sealed record ThumbnailReservation(
    Guid JobId,
    Guid MediaId,
    StorageObjectLocation Location,
    string OriginalFileName,
    ActorReference UploadedBy);

public sealed record MediaUploadFinalizationResult(
    string ThumbnailStatus,
    Guid? ThumbnailMediaId,
    Guid? ThumbnailJobId);
