// File: backend/Services/Media/MediaService.Application/Services/Storage/IStorage.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

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

    Task<StorageObjectInfo> PromoteAsync(
        StoragePromotionRequest request,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken);
}
