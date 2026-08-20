// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/CreateIntent/CreateUploadIntentHandler.cs
// Mục đích: Điều phối use case CreateUploadIntentHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.Media.DirectUpload.CreateIntent;

public sealed class CreateUploadIntentHandler(
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
        var contentType = MediaContentTypeRules.Normalize(command.ContentType);
        var category = MediaContentTypeRules.InferCategory(contentType);
        var mediaType = MediaContentTypeRules.ToMediaType(category);

        if (command.SizeBytes <= 0)
        {
            throw MediaErrors.InvalidMedia("The file must not be empty.");
        }

        if (command.SizeBytes > uploadOptions.GetMaxBytes(category))
        {
            throw MediaErrors.PayloadTooLarge();
        }

        var checksum = DirectUploadChecksum.Validate(command.ChecksumSha256);
        var actor = command.UploadedBy.Normalize();
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
            mediaId, mediaType, MediaObjectStatuses.Pending, true, policy.ExpiresAtUtc,
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

}
