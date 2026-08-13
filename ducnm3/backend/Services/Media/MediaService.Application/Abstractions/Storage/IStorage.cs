namespace MediaService.Application.Abstractions.Storage;

public interface IStorage
{
    Task<StorageObjectInfo> UploadAsync(
        StorageUploadRequest request,
        CancellationToken cancellationToken);

    Task<StorageObjectInfo> DownloadAsync(
        StorageDownloadRequest request,
        CancellationToken cancellationToken);

    Task<StorageObjectInfo> GetMetadataAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken);
}
