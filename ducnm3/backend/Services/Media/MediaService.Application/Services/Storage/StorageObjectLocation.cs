// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageObjectLocation.cs
// Mục đích: Đóng gói vị trí bucket và object key của media trong storage.

namespace MediaService.Application.Services.Storage;

public sealed record StorageObjectLocation(
    string Bucket,
    string ObjectKey);
