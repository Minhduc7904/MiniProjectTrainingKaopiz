namespace MediaService.Application.Storage;

public sealed record StorageUploadRequest(
    StorageMediaCategory Category,
    string ContentType,
    string Extension,
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
    string? ETag);

public sealed record StorageDownloadRequest(
    StorageObjectLocation Location,
    Stream Destination);

public sealed record StorageHealthProbeResult(bool IsHealthy);
