// File: backend/Services/Media/MediaService.Application/Services/Storage/IStorageHealthProbe.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public interface IStorageHealthProbe
{
    Task<StorageHealthProbeResult> CheckAsync(CancellationToken cancellationToken);
}
