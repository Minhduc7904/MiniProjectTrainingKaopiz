// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageUploadRequest.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed record StorageUploadRequest(
    StorageObjectLocation Location,
    string ContentType,
    Stream Content,
    long Size);
