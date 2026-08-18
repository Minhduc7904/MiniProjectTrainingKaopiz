// File: backend/Services/Media/MediaService.Domain/Constants/MediaUsageTypes.cs
// Mục đích: Khai báo các kiểu sử dụng Media để xác định ý nghĩa liên kết giữa media và owner.

namespace MediaService.Domain.Constants;

public static class MediaUsageTypes
{
    public const string Attachment = "ATTACHMENT";
    public const string Avatar = "AVATAR";
    public const string Embed = "EMBED";
    public const string Thumbnail = "THUMBNAIL";
}
