// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageObjectInfo.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed record StorageObjectInfo(
    string Bucket,
    string ObjectKey,
    string ContentType,
    long Size,
    string? ETag,
    string? ChecksumSha256 = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
