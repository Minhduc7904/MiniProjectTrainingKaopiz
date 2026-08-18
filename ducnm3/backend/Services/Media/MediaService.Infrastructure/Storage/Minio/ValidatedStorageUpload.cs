// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/ValidatedStorageUpload.cs
// Mục đích: Mang file upload cùng metadata đã validate vào adapter MinIO để ghi object đúng content type và checksum.

namespace MediaService.Infrastructure.Storage.Minio;

public sealed record ValidatedStorageUpload(string ContentType);
