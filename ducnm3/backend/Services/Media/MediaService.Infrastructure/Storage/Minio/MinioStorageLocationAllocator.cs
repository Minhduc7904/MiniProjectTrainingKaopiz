// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/MinioStorageLocationAllocator.cs
// Mục đích: Chọn bucket và sinh vị trí MinIO cho file Media theo category trước khi upload hoặc promote.

using MediaService.Application.Services.Storage;
using Microsoft.Extensions.Options;

namespace MediaService.Infrastructure.Storage.Minio;

public sealed class MinioStorageLocationAllocator(
    IOptions<MinioStorageOptions> options,
    MinioObjectKeyGenerator objectKeyGenerator) : IStorageLocationAllocator
{
    private readonly MinioStorageOptions storageOptions = options.Value;

    public StorageObjectLocation Allocate(
        StorageMediaCategory category,
        string extension)
    {
        if (!Enum.IsDefined(category))
        {
            throw new StorageValidationException(
                $"Unsupported media category '{category}'.");
        }

        var normalizedExtension =
            MinioStorageRequestValidator.NormalizeExtension(extension);
        return new StorageObjectLocation(
            storageOptions.GetBucket(category),
            objectKeyGenerator.Create(normalizedExtension));
    }
}
