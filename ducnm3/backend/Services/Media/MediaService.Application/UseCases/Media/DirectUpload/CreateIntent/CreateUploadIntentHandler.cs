// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/CreateIntent/CreateUploadIntentHandler.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.Services.Actors;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Domain.Constants;

using MediaService.Application.Common.Errors;

namespace MediaService.Application.UseCases.Media.DirectUpload.CreateIntent;

public sealed class CreateUploadIntentHandler(
    IActorValidationService actorValidationService,
    IStorageLocationAllocator locationAllocator,
    IStorageUploadPolicyProvider policyProvider,
    IMediaRepository mediaRepository,
    MediaUploadOptions uploadOptions)
{
    public async Task<CreateUploadIntentResult> HandleAsync(
        CreateUploadIntentCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var fileName = ValidateFileName(command.OriginalFileName);
        var category = ParseCategory(command.MediaType);
        var mediaType = command.MediaType.Trim().ToUpperInvariant();
        var contentType = MediaContentTypeRules.Normalize(command.ContentType);
        if (!MediaContentTypeRules.Matches(category, contentType))
        {
            throw MediaErrors.UnsupportedMediaType();
        }

        if (command.SizeBytes <= 0)
        {
            throw MediaErrors.InvalidMedia("The file must not be empty.");
        }

        if (command.SizeBytes > uploadOptions.GetMaxBytes(category))
        {
            throw MediaErrors.PayloadTooLarge();
        }

        var checksum = DirectUploadChecksum.Validate(command.ChecksumSha256);
        var actor = await actorValidationService.ValidateAsync(
            command.UploadedBy,
            cancellationToken);
        StorageObjectLocation location;
        try
        {
            location = locationAllocator.Allocate(category, Path.GetExtension(fileName));
        }
        catch (StorageValidationException exception)
        {
            throw MediaErrors.InvalidMedia(exception.Message);
        }

        var mediaId = Guid.NewGuid();
        await mediaRepository.AddPendingAsync(
            new PendingMediaRecord(
                mediaId, location, mediaType, contentType, fileName,
                command.SizeBytes, actor, true, null, checksum),
            cancellationToken);

        var policy = await policyProvider.CreateAsync(
            new StorageUploadPolicyRequest(
                location, contentType, command.SizeBytes, checksum),
            cancellationToken);
        return new CreateUploadIntentResult(
            mediaId, MediaObjectStatuses.Pending, true, policy.ExpiresAtUtc,
            policy.UploadUrl, policy.FormFields);
    }

    private static string ValidateFileName(string originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw MediaErrors.InvalidMedia("originalFileName is required.");
        }

        var fileName = Path.GetFileName(originalFileName);
        if (!string.Equals(fileName, originalFileName, StringComparison.Ordinal) ||
            fileName.Length > 255 || fileName.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
        {
            throw MediaErrors.InvalidMedia("The file name is invalid.");
        }

        return fileName;
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
}
