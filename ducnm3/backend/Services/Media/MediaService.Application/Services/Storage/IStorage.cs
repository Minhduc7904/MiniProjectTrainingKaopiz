// File: backend/Services/Media/MediaService.Application/Services/Storage/IStorage.cs
// Mục đích: Định nghĩa port lưu trữ object cho upload, đọc metadata, xóa và promote file; Application không phụ thuộc MinIO.

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
