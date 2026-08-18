// File: backend/Services/Media/MediaService.Application/Services/Storage/IStorageUploadPolicyProvider.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public interface IStorageUploadPolicyProvider
{
    Task<StorageUploadPolicy> CreateAsync(
        StorageUploadPolicyRequest request,
        CancellationToken cancellationToken);
}
