using MediaService.Application.Actors;
using MediaService.Application.Persistence;
using MediaService.Application.Storage;
using MediaService.Domain;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Upload;

public sealed class MediaUploadOptions
{
    public const string SectionName = "MediaUpload";

    public long ImageMaxBytes { get; init; } = 10 * 1024 * 1024;

    public long VideoMaxBytes { get; init; } = 500 * 1024 * 1024;

    public long DocumentMaxBytes { get; init; } = 50 * 1024 * 1024;

    public long AudioMaxBytes { get; init; } = 100 * 1024 * 1024;

    public long OtherMaxBytes { get; init; } = 25 * 1024 * 1024;

    public long RequestMaxBytes { get; init; } = 525 * 1024 * 1024;

    public long GetMaxBytes(StorageMediaCategory category) =>
        category switch
        {
            StorageMediaCategory.Image => ImageMaxBytes,
            StorageMediaCategory.Video => VideoMaxBytes,
            StorageMediaCategory.Document => DocumentMaxBytes,
            StorageMediaCategory.Audio => AudioMaxBytes,
            StorageMediaCategory.Other => OtherMaxBytes,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

    public void Validate()
    {
        if (ImageMaxBytes <= 0 ||
            VideoMaxBytes <= 0 ||
            DocumentMaxBytes <= 0 ||
            AudioMaxBytes <= 0 ||
            OtherMaxBytes <= 0 ||
            RequestMaxBytes < VideoMaxBytes)
        {
            throw new InvalidOperationException(
                "MediaUpload limits must be positive and request max must cover video max.");
        }
    }
}

public sealed record UploadMediaCommand(
    string MediaType,
    string ContentType,
    string OriginalFileName,
    Stream Content,
    long SizeBytes,
    ActorReference UploadedBy);

public sealed record UploadMediaResult(
    Guid Id,
    string MediaType,
    string ContentType,
    long SizeBytes,
    string Status,
    string ContentUrl);

public sealed partial class UploadMediaHandler(
    IActorValidationService actorValidationService,
    IStorageLocationAllocator locationAllocator,
    IStorage storage,
    IMediaRepository mediaRepository,
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
            throw new MediaApplicationException(
                MediaErrorCodes.InvalidMedia,
                exception.Message,
                400);
        }

        var mediaId = Guid.NewGuid();
        await mediaRepository.AddPendingAsync(
            new PendingMediaRecord(
                mediaId,
                location,
                command.MediaType.Trim().ToUpperInvariant(),
                NormalizeContentType(command.ContentType),
                Path.GetFileName(command.OriginalFileName),
                command.SizeBytes,
                actor),
            cancellationToken);

        StorageObjectInfo uploadedObject;
        try
        {
            uploadedObject = await storage.UploadAsync(
                new StorageUploadRequest(
                    location,
                    command.ContentType,
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
            throw new MediaApplicationException(
                MediaErrorCodes.MediaUploadFailed,
                "Media upload failed.",
                503);
        }

        if (string.IsNullOrWhiteSpace(uploadedObject.ChecksumSha256))
        {
            await CompensateAsync(
                mediaId,
                location,
                "Storage checksum was not produced.",
                CancellationToken.None);
            throw new MediaApplicationException(
                MediaErrorCodes.MediaUploadFailed,
                "Media upload failed.",
                503);
        }

        try
        {
            await mediaRepository.MarkReadyAsync(
                mediaId,
                uploadedObject.ChecksumSha256,
                timeProvider.GetUtcNow().UtcDateTime,
                cancellationToken);
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

        return new UploadMediaResult(
            mediaId,
            command.MediaType.Trim().ToUpperInvariant(),
            uploadedObject.ContentType,
            uploadedObject.Size,
            MediaObjectStatuses.Ready,
            $"/api/media/{mediaId}/content");
    }

    private StorageMediaCategory ValidateAndGetCategory(UploadMediaCommand command)
    {
        if (command.Content is null || !command.Content.CanRead)
        {
            throw InvalidMedia("A readable file is required.");
        }

        if (string.IsNullOrWhiteSpace(command.OriginalFileName) ||
            string.IsNullOrWhiteSpace(Path.GetExtension(command.OriginalFileName)))
        {
            throw InvalidMedia("A file name with an extension is required.");
        }

        if (command.SizeBytes <= 0)
        {
            throw InvalidMedia("The file must not be empty.");
        }

        var category = ParseCategory(command.MediaType);
        var contentType = NormalizeContentType(command.ContentType);
        if (!StorageMediaTypeRules.Matches(category, contentType))
        {
            throw new MediaApplicationException(
                MediaErrorCodes.UnsupportedMediaType,
                "The content type does not match mediaType.",
                415);
        }

        if (command.SizeBytes > uploadOptions.GetMaxBytes(category))
        {
            throw new MediaApplicationException(
                MediaErrorCodes.MediaTooLarge,
                "The file exceeds the configured size limit.",
                413);
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
            throw InvalidMedia("mediaType is required.");
        }

        return mediaType.Trim().ToUpperInvariant() switch
        {
            MediaTypes.Image => StorageMediaCategory.Image,
            MediaTypes.Video => StorageMediaCategory.Video,
            MediaTypes.Document => StorageMediaCategory.Document,
            MediaTypes.Audio => StorageMediaCategory.Audio,
            MediaTypes.Other => StorageMediaCategory.Other,
            _ => throw new MediaApplicationException(
                MediaErrorCodes.InvalidMedia,
                "mediaType is not supported.",
                400)
        };
    }

    private static string NormalizeContentType(string contentType) =>
        string.IsNullOrWhiteSpace(contentType)
            ? throw InvalidMedia("Content type is required.")
            : contentType.Split(';', 2)[0].Trim().ToLowerInvariant();

    private static MediaApplicationException InvalidMedia(string message) =>
        new(MediaErrorCodes.InvalidMedia, message, 400);

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
