// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageHealthProbeResult.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed record StorageHealthProbeResult(bool IsHealthy);
