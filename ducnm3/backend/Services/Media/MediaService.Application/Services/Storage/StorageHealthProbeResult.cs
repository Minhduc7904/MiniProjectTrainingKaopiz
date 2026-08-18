// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageHealthProbeResult.cs
// Mục đích: Định nghĩa dữ liệu đầu ra của use case StorageHealthProbeResult.

namespace MediaService.Application.Services.Storage;

public sealed record StorageHealthProbeResult(bool IsHealthy);
