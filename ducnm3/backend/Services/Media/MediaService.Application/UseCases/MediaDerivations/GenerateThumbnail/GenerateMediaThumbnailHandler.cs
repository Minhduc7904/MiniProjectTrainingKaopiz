// File: backend/Services/Media/MediaService.Application/UseCases/MediaDerivations/GenerateThumbnail/GenerateMediaThumbnailHandler.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Services.Derivation;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.Contracts.Messaging;
using MediaService.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.UseCases.MediaDerivations.GenerateThumbnail;

public sealed partial class GenerateMediaThumbnailHandler(
    IMediaDerivationRepository repository,
    IStorage storage,
    ITemporaryMediaFileFactory temporaryFileFactory,
    IThumbnailGenerator thumbnailGenerator,
    TimeProvider timeProvider,
    ILogger<GenerateMediaThumbnailHandler> logger)
{
    public async Task HandleAsync(
        GenerateMediaThumbnailV1 command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var work = await repository.BeginAsync(
            command.JobId,
            command.SourceMediaId,
            command.DerivativeMediaId,
            cancellationToken) ?? throw new InvalidOperationException(
                "The thumbnail derivation job does not exist.");

        if (work.JobStatus == MediaDerivationStatuses.Ready &&
            work.DerivativeStatus == MediaObjectStatuses.Ready)
        {
            return;
        }

        var uploaded = false;
        try
        {
            await using var temporaryFile =
                await temporaryFileFactory.CreateAsync(cancellationToken);
            await storage.DownloadAsync(
                new StorageDownloadRequest(
                    work.SourceLocation,
                    temporaryFile.Stream),
                cancellationToken);
            await temporaryFile.Stream.FlushAsync(cancellationToken);

            var generated = await thumbnailGenerator.GenerateAsync(
                new ThumbnailGenerationRequest(
                    temporaryFile.Path,
                    work.SourceMediaType,
                    work.SourceContentType),
                cancellationToken);
            await using var content = new MemoryStream(
                generated.Content,
                writable: false);
            var uploadedObject = await storage.UploadAsync(
                new StorageUploadRequest(
                    work.DerivativeLocation,
                    "image/webp",
                    content,
                    generated.Content.LongLength),
                cancellationToken);
            uploaded = true;
            if (string.IsNullOrWhiteSpace(uploadedObject.ChecksumSha256))
            {
                throw new InvalidOperationException(
                    "Storage did not return a checksum for the generated thumbnail.");
            }

            await repository.CompleteAsync(
                work.JobId,
                work.DerivativeMediaId,
                uploadedObject.ChecksumSha256,
                uploadedObject.Size,
                timeProvider.GetUtcNow().UtcDateTime,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            if (uploaded)
            {
                await TryDeleteThumbnailAsync(
                    work.DerivativeLocation,
                    CancellationToken.None);
            }

            LogGenerationFailed(logger, work.JobId, work.SourceMediaId, exception);
            throw;
        }
    }

    private async Task TryDeleteThumbnailAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken)
    {
        try
        {
            await storage.DeleteAsync(location, cancellationToken);
        }
        catch (Exception exception)
        {
            LogCompensationFailed(logger, exception);
        }
    }

    [LoggerMessage(
        EventId = 2301,
        Level = LogLevel.Error,
        Message = "Thumbnail generation failed for job {JobId} and source media {SourceMediaId}.")]
    private static partial void LogGenerationFailed(
        ILogger logger,
        Guid jobId,
        Guid sourceMediaId,
        Exception exception);

    [LoggerMessage(
        EventId = 2302,
        Level = LogLevel.Error,
        Message = "Failed to delete a generated thumbnail during compensation.")]
    private static partial void LogCompensationFailed(
        ILogger logger,
        Exception exception);

}
