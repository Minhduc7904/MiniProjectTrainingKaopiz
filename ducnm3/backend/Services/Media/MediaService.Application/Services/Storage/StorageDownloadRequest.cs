// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageDownloadRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho StorageDownloadRequest.

namespace MediaService.Application.Services.Storage;

public sealed record StorageDownloadRequest(
    StorageObjectLocation Location,
    Stream Destination,
    long Offset = 0,
    long? Length = null);
