// File: backend/Services/Media/MediaService.Application/UseCases/Media/Upload/UploadMediaHandler.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.Services.Actors;
using MediaService.Application.UseCases.MediaDerivations.GenerateThumbnail;
using MediaService.Application.UseCases.Media.DirectUpload;
using MediaService.Domain.Constants;
using Microsoft.Extensions.Logging;

using MediaService.Application.Common.Errors;

namespace MediaService.Application.UseCases.Media.Upload;

public sealed partial class UploadMediaHandler(
    IActorValidationService actorValidationService,
    IStorageLocationAllocator locationAllocator,
    IStorage storage,
    IMediaRepository mediaRepository,
    IMediaUploadFinalizer uploadFinalizer,
    MediaUploadOptions uploadOptions,
    TimeProvider timeProvider,
    ILogger<UploadMediaHandler> logger)
{
    public async Task<UploadMediaResult> HandleAsync(
        UploadMediaCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var category = ValidateAndGetCategory(command);
        var normalizedMediaType = command.MediaType.Trim().ToUpperInvariant();
        var normalizedContentType =
            MediaContentTypeRules.Normalize(command.ContentType);
        var actor = await actorValidationService.ValidateAsync(
            command.UploadedBy,
            cancellationToken);
        var extension = Path.GetExtension(command.OriginalFileName);
        StorageObjectLocation location;
        try
        {
            location = locationAllocator.Allocate(category, extension);
        }
        catch (StorageValidationException exception)
        {
            throw MediaErrors.InvalidMedia(exception.Message);
        }

        var mediaId = Guid.NewGuid();
        ThumbnailReservation? thumbnail = null;
        if (MediaThumbnailPolicy.RequiresThumbnail(
                normalizedMediaType,
                normalizedContentType))
        {
            var thumbnailMediaId = Guid.NewGuid();
            thumbnail = new ThumbnailReservation(
                Guid.NewGuid(),
                thumbnailMediaId,
                locationAllocator.Allocate(StorageMediaCategory.Image, ".webp"),
                $"{Path.GetFileNameWithoutExtension(command.OriginalFileName)}.thumbnail.webp",
                actor);
        }

        await mediaRepository.AddPendingAsync(
            new PendingMediaRecord(
                mediaId,
                location,
                normalizedMediaType,
                normalizedContentType,
                Path.GetFileName(command.OriginalFileName),
                command.SizeBytes,
                actor,
                true,
                null),
            cancellationToken);

        StorageObjectInfo uploadedObject;
        try
        {
            uploadedObject = await storage.UploadAsync(
                new StorageUploadRequest(
                    location,
                    normalizedContentType,
                    command.Content,
                    command.SizeBytes),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await CompensateAsync(
                mediaId,
                location,
                "Upload was cancelled.",
                CancellationToken.None);
            throw;
        }
        catch (Exception exception) when (
            exception is StorageOperationException or StorageValidationException)
        {
            await CompensateAsync(
                mediaId,
                location,
                "Storage upload failed.",
                CancellationToken.None);
            throw MediaErrors.UploadFailed();
        }

        if (string.IsNullOrWhiteSpace(uploadedObject.ChecksumSha256))
        {
            await CompensateAsync(
                mediaId,
                location,
                "Storage checksum was not produced.",
                CancellationToken.None);
            throw MediaErrors.UploadFailed();
        }

        try
        {
            var finalization = await uploadFinalizer.FinalizeAsync(
                new MediaUploadFinalizationRequest(
                    mediaId,
                    normalizedMediaType,
                    normalizedContentType,
                    uploadedObject.ChecksumSha256,
                    timeProvider.GetUtcNow().UtcDateTime,
                    thumbnail),
                cancellationToken);

            return new UploadMediaResult(
                mediaId,
                normalizedMediaType,
                uploadedObject.ContentType,
                uploadedObject.Size,
                MediaObjectStatuses.Ready,
                true,
                finalization.CompletedAtUtc,
                finalization.ThumbnailStatus,
                finalization.ThumbnailMediaId,
                finalization.ThumbnailJobId);
        }
        catch (Exception)
        {
            await CompensateAsync(
                mediaId,
                location,
                "Database finalization failed.",
                CancellationToken.None);
            throw;
        }
    }

    private StorageMediaCategory ValidateAndGetCategory(UploadMediaCommand command)
    {
        if (command.Content is null || !command.Content.CanRead)
        {
            throw MediaErrors.InvalidMedia("A readable file is required.");
        }

        if (string.IsNullOrWhiteSpace(command.OriginalFileName) ||
            string.IsNullOrWhiteSpace(Path.GetExtension(command.OriginalFileName)))
        {
            throw MediaErrors.InvalidMedia(
                "A file name with an extension is required.");
        }

        var safeFileName = Path.GetFileName(command.OriginalFileName);
        if (safeFileName.Length > 500 ||
            safeFileName.Any(char.IsControl))
        {
            throw MediaErrors.InvalidMedia("The file name is invalid.");
        }

        if (command.SizeBytes <= 0)
        {
            throw MediaErrors.InvalidMedia("The file must not be empty.");
        }

        var category = ParseCategory(command.MediaType);
        var contentType = MediaContentTypeRules.Normalize(command.ContentType);
        if (!MediaContentTypeRules.Matches(category, contentType))
        {
            throw MediaErrors.UnsupportedMediaType();
        }

        if (command.SizeBytes > uploadOptions.GetMaxBytes(category))
        {
            throw MediaErrors.PayloadTooLarge();
        }

        return category;
    }

    private async Task CompensateAsync(
        Guid mediaId,
        StorageObjectLocation location,
        string failureReason,
        CancellationToken cancellationToken)
    {
        try
        {
            await storage.DeleteAsync(location, cancellationToken);
        }
        catch (Exception exception)
        {
            LogCompensationDeleteFailed(logger, mediaId, exception);
        }

        try
        {
            await mediaRepository.MarkFailedAsync(
                mediaId,
                failureReason,
                cancellationToken);
        }
        catch (Exception exception)
        {
            LogMarkFailedFailed(logger, mediaId, exception);
        }
    }

    private static StorageMediaCategory ParseCategory(string mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaType))
        {
            throw MediaErrors.InvalidMedia("mediaType is required.");
        }

        return mediaType.Trim().ToUpperInvariant() switch
        {
            MediaTypes.Image => StorageMediaCategory.Image,
            MediaTypes.Video => StorageMediaCategory.Video,
            MediaTypes.Document => StorageMediaCategory.Document,
            MediaTypes.Audio => StorageMediaCategory.Audio,
            MediaTypes.Other => StorageMediaCategory.Other,
            _ => throw MediaErrors.InvalidMedia("mediaType is not supported.")
        };
    }

    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Error,
        Message = "Failed to delete compensated storage object for media {MediaId}.")]
    private static partial void LogCompensationDeleteFailed(
        ILogger logger,
        Guid mediaId,
        Exception exception);

    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Error,
        Message = "Failed to mark media {MediaId} as FAILED.")]
    private static partial void LogMarkFailedFailed(
        ILogger logger,
        Guid mediaId,
        Exception exception);
}
