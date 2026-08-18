// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/MinioStorageOptions.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Services.Storage;

namespace MediaService.Infrastructure.Storage.Minio;

public sealed class MinioStorageOptions
{
    public const string SectionName = "Storage:Minio";

    public string Endpoint { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public bool UseSsl { get; init; }

    public string PublicEndpoint { get; init; } = "localhost:9000";

    public bool PublicUseSsl { get; init; }

    public int UploadPresignExpirySeconds { get; init; } = 900;

    public string ImageBucket { get; init; } = string.Empty;

    public string VideoBucket { get; init; } = string.Empty;

    public string DocumentBucket { get; init; } = string.Empty;

    public string AudioBucket { get; init; } = string.Empty;

    public string OtherBucket { get; init; } = string.Empty;

    public int HealthTimeoutSeconds { get; init; } = 3;

    public string GetBucket(StorageMediaCategory category) =>
        category switch
        {
            StorageMediaCategory.Image => ImageBucket,
            StorageMediaCategory.Video => VideoBucket,
            StorageMediaCategory.Document => DocumentBucket,
            StorageMediaCategory.Audio => AudioBucket,
            StorageMediaCategory.Other => OtherBucket,
            _ => throw new StorageValidationException($"Unsupported media category '{category}'.")
        };

    public IReadOnlyCollection<string> GetBuckets() =>
        [ImageBucket, VideoBucket, DocumentBucket, AudioBucket, OtherBucket];
}
