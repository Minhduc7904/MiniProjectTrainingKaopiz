namespace MediaService.Application.Storage;

public sealed record StorageUploadRequest(
    StorageObjectLocation Location,
    string ContentType,
    Stream Content,
    long Size);

public sealed record StorageObjectLocation(
    string Bucket,
    string ObjectKey);

public sealed record StorageObjectInfo(
    string Bucket,
    string ObjectKey,
    string ContentType,
    long Size,
    string? ETag,
    string? ChecksumSha256 = null);

public sealed record StorageDownloadRequest(
    StorageObjectLocation Location,
    Stream Destination);

public sealed record StorageHealthProbeResult(bool IsHealthy);
