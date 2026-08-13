namespace MediaService.Application.Abstractions.Storage;

public sealed record StorageObjectInfo(
    string Bucket,
    string ObjectKey,
    string ContentType,
    long Size,
    string? ETag,
    string? ChecksumSha256 = null);
