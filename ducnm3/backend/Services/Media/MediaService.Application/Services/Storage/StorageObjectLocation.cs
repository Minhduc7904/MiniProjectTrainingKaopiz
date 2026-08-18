// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageObjectLocation.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed record StorageObjectLocation(
    string Bucket,
    string ObjectKey);
