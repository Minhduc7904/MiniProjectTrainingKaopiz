namespace MediaService.Application.Abstractions.Storage;

public sealed record StorageObjectLocation(
    string Bucket,
    string ObjectKey);
