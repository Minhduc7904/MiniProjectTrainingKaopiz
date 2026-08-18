// File: backend/Services/Media/MediaService.Application/Services/Storage/StoragePromotionRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho StoragePromotionRequest.

namespace MediaService.Application.Services.Storage;

public sealed record StoragePromotionRequest(
    StorageObjectLocation Source,
    StorageObjectLocation Destination,
    string SourceETag);
