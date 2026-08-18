// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/Complete/CompleteDirectUploadHandler.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.Services.Actors;
using MediaService.Application.UseCases.MediaDerivations.GenerateThumbnail;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Domain.Constants;

using MediaService.Application.Common.Errors;

namespace MediaService.Application.UseCases.Media.DirectUpload.Complete;

public sealed class CompleteDirectUploadHandler(
    IActorValidationService actorValidationService,
    IStorage storage,
    IMediaRepository mediaRepository,
    IMediaUploadFinalizer uploadFinalizer,
    IStorageLocationAllocator locationAllocator,
    TimeProvider timeProvider)
{
    public async Task<UploadMediaResult> HandleAsync(
        CompleteDirectUploadCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.MediaId == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia("mediaId must be a non-empty UUID.");
        }

        var actor = await actorValidationService.ValidateAsync(
            command.UploadedBy,
            cancellationToken);
        var media = await mediaRepository.GetByIdAsync(
            command.MediaId,
            cancellationToken) ?? throw MediaErrors.MediaNotFound();
        if (media.DeletedAtUtc is not null || media.UploadedBy != actor)
        {
            throw MediaErrors.MediaNotFound();
        }

        ThumbnailReservation? thumbnail = null;
        StorageObjectLocation? finalLocation = null;
        if (media.Status == MediaObjectStatuses.Pending)
        {
            var verifiedMetadata = await ReadAndVerifyMetadataAsync(media, cancellationToken);
            if (string.IsNullOrWhiteSpace(verifiedMetadata.ETag))
            {
                throw MediaErrors.DirectUploadIncomplete();
            }
            finalLocation = locationAllocator.Allocate(
                ParseCategory(media.MediaType),
                Path.GetExtension(media.OriginalFileName));
            try
            {
                await storage.PromoteAsync(
                    new StoragePromotionRequest(
                        media.Location,
                        finalLocation,
                        verifiedMetadata.ETag),
                    cancellationToken);
            }
            catch (StorageObjectNotFoundException)
            {
                throw MediaErrors.DirectUploadIncomplete();
            }
            catch (StorageOperationException)
            {
                throw MediaErrors.StorageUnavailable();
            }
            if (MediaThumbnailPolicy.RequiresThumbnail(media.MediaType, media.ContentType))
            {
                thumbnail = new ThumbnailReservation(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    locationAllocator.Allocate(StorageMediaCategory.Image, ".webp"),
                    $"{Path.GetFileNameWithoutExtension(media.OriginalFileName)}.thumbnail.webp",
                    actor);
            }

        }
        else if (media.Status != MediaObjectStatuses.Ready)
        {
            throw MediaErrors.DirectUploadIncomplete();
        }

        var completedAt = timeProvider.GetUtcNow().UtcDateTime;
        MediaUploadFinalizationResult finalization;
        try
        {
            finalization = await uploadFinalizer.FinalizeAsync(
                new MediaUploadFinalizationRequest(
                    media.Id,
                    media.MediaType,
                    media.ContentType,
                    media.ChecksumSha256 ?? throw MediaErrors.DirectUploadIncomplete(),
                    completedAt,
                    thumbnail,
                    actor,
                    finalLocation),
                cancellationToken);
        }
        catch
        {
            // Commit outcome is ambiguous. Preserve the unique promoted object so
            // P5-20 can delete it only after checking the durable database reference.
            throw;
        }

        if (finalLocation is not null)
        {
            await BestEffortDeleteAsync(
                finalization.Transitioned ? media.Location : finalLocation);
        }

        return new UploadMediaResult(
            media.Id,
            media.MediaType,
            media.ContentType,
            media.SizeBytes,
            MediaObjectStatuses.Ready,
            true,
            finalization.CompletedAtUtc,
            finalization.ThumbnailStatus,
            finalization.ThumbnailMediaId,
            finalization.ThumbnailJobId);
    }

    private async Task<StorageObjectInfo> ReadAndVerifyMetadataAsync(
        MediaRecord media,
        CancellationToken cancellationToken)
    {
        StorageObjectInfo metadata;
        try
        {
            metadata = await storage.GetMetadataAsync(media.Location, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (StorageObjectNotFoundException)
        {
            throw MediaErrors.DirectUploadIncomplete();
        }
        catch (StorageOperationException)
        {
            throw MediaErrors.StorageUnavailable();
        }

        string actualContentType;
        try
        {
            actualContentType = MediaContentTypeRules.Normalize(metadata.ContentType);
        }
        catch (MediaApplicationException)
        {
            throw MediaErrors.DirectUploadIncomplete();
        }

        string? declaredChecksum = null;
        var hasChecksum = metadata.Metadata?.TryGetValue(
            DirectUploadChecksum.MetadataKey,
            out declaredChecksum) == true;
        if (metadata.Size != media.SizeBytes ||
            !string.Equals(actualContentType, media.ContentType, StringComparison.Ordinal) ||
            !hasChecksum ||
            !string.Equals(declaredChecksum, media.ChecksumSha256, StringComparison.Ordinal))
        {
            throw MediaErrors.DirectUploadIncomplete();
        }

        return metadata;
    }

    private static StorageMediaCategory ParseCategory(string mediaType) =>
        mediaType switch
        {
            MediaTypes.Image => StorageMediaCategory.Image,
            MediaTypes.Video => StorageMediaCategory.Video,
            MediaTypes.Document => StorageMediaCategory.Document,
            MediaTypes.Audio => StorageMediaCategory.Audio,
            MediaTypes.Other => StorageMediaCategory.Other,
            _ => throw MediaErrors.InvalidMedia("mediaType is not supported.")
        };

    private async Task BestEffortDeleteAsync(StorageObjectLocation location)
    {
        try
        {
            await storage.DeleteAsync(location, CancellationToken.None);
        }
        catch (Exception)
        {
            // Cleanup is intentionally best effort after promotion/finalization.
        }
    }
}
