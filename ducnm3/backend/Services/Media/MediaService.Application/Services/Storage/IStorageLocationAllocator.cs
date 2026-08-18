// File: backend/Services/Media/MediaService.Application/Services/Storage/IStorageLocationAllocator.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public interface IStorageLocationAllocator
{
    StorageObjectLocation Allocate(
        StorageMediaCategory category,
        string extension);
}
