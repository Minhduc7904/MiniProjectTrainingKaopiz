// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/MinioUploadPolicyProvider.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Services.Storage;
using MediaService.Application.UseCases.Media.DirectUpload;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;

namespace MediaService.Infrastructure.Storage.Minio;

public sealed class MinioUploadPolicyProvider(
    MinioSigningClient signingClientRegistration,
    IOptions<MinioStorageOptions> options,
    TimeProvider timeProvider) : IStorageUploadPolicyProvider
{
    private readonly MinioStorageOptions storageOptions = options.Value;
    private readonly IMinioClient signingClient = signingClientRegistration.Client;

    public async Task<StorageUploadPolicy> CreateAsync(
        StorageUploadPolicyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        MinioStorageRequestValidator.ValidateLocation(
            request.Location,
            storageOptions.GetBuckets());
        var expiresAt = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(
            storageOptions.UploadPresignExpirySeconds);
        var policy = new PostPolicy();
        policy.SetBucket(request.Location.Bucket);
        policy.SetKey(request.Location.ObjectKey);
        policy.SetContentType(request.ContentType);
        policy.SetContentLength(request.SizeBytes);
        policy.SetUserMetadata(
            DirectUploadChecksum.MetadataKey,
            request.ChecksumSha256);
        policy.SetExpires(expiresAt);

        var (uploadUrl, formData) =
            await signingClient.PresignedPostPolicyAsync(policy);
        var formFields = new Dictionary<string, string>(formData, StringComparer.Ordinal)
        {
            ["Content-Type"] = request.ContentType,
        };
        return new StorageUploadPolicy(
            uploadUrl,
            formFields,
            expiresAt);
    }
}
