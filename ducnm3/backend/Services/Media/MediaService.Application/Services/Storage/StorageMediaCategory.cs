// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageMediaCategory.cs
// Mục đích: Khai báo nhóm media dùng để áp dụng giới hạn dung lượng và loại nội dung khi upload.

namespace MediaService.Application.Services.Storage;

public enum StorageMediaCategory
{
    Image,
    Video,
    Document,
    Audio,
    Other
}
