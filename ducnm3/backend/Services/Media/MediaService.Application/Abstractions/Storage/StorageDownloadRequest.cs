namespace MediaService.Application.Abstractions.Storage;

public sealed record StorageDownloadRequest(
    StorageObjectLocation Location,
    Stream Destination);
