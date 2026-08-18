// File: backend/Services/Media/MediaService.Application/Services/Storage/StoragePromotionRequest.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed record StoragePromotionRequest(
    StorageObjectLocation Source,
    StorageObjectLocation Destination,
    string SourceETag);
