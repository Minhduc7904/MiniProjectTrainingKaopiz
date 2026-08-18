// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageUploadRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho StorageUploadRequest.

namespace MediaService.Application.Services.Storage;

public sealed record StorageUploadRequest(
    StorageObjectLocation Location,
    string ContentType,
    Stream Content,
    long Size);
