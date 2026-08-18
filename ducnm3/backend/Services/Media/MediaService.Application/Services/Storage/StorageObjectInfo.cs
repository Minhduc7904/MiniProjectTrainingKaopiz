// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageObjectInfo.cs
// Mục đích: Mô tả metadata object đọc từ storage, gồm kích thước, content type, checksum và ETag để xác minh upload.

namespace MediaService.Application.Services.Storage;

public sealed record StorageObjectInfo(
    string Bucket,
    string ObjectKey,
    string ContentType,
    long Size,
    string? ETag,
    string? ChecksumSha256 = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
