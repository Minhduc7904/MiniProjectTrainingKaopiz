// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageDownloadRequest.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed record StorageDownloadRequest(
    StorageObjectLocation Location,
    Stream Destination);
