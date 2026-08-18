// File: backend/Services/Media/MediaService.Application/Services/Storage/IStorageHealthProbe.cs
// Mục đích: Định nghĩa port kiểm tra tình trạng storage để health check không phụ thuộc nhà cung cấp cụ thể.

namespace MediaService.Application.Services.Storage;

public interface IStorageHealthProbe
{
    Task<StorageHealthProbeResult> CheckAsync(CancellationToken cancellationToken);
}
