// File: backend/Services/Media/MediaService.Application/Services/Storage/IStorageLocationAllocator.cs
// Mục đích: Định nghĩa port cấp bucket và object key riêng cho media upload, tránh use case tự tạo đường dẫn storage.

namespace MediaService.Application.Services.Storage;

public interface IStorageLocationAllocator
{
    StorageObjectLocation Allocate(
        StorageMediaCategory category,
        string extension);
}
