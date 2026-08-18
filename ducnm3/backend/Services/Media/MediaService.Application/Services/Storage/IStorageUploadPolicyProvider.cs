// File: backend/Services/Media/MediaService.Application/Services/Storage/IStorageUploadPolicyProvider.cs
// Mục đích: Định nghĩa port tạo chính sách presigned POST cho upload trực tiếp mà không lộ chi tiết MinIO vào use case.

namespace MediaService.Application.Services.Storage;

public interface IStorageUploadPolicyProvider
{
    Task<StorageUploadPolicy> CreateAsync(
        StorageUploadPolicyRequest request,
        CancellationToken cancellationToken);
}
