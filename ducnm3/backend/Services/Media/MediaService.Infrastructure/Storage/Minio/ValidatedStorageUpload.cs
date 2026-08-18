// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/ValidatedStorageUpload.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Infrastructure.Storage.Minio;

public sealed record ValidatedStorageUpload(string ContentType);
