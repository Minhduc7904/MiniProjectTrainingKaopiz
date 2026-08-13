namespace MediaService.Application.Abstractions.Storage;

public sealed record StorageUploadRequest(
    StorageObjectLocation Location,
    string ContentType,
    Stream Content,
    long Size);
