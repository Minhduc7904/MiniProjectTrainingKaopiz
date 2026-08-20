// File: backend/Services/Media/MediaService.Domain/Constants/MediaTypes.cs
// Mục đích: Khai báo các nhóm Media Type được chấp nhận để validate loại file và áp dụng quy tắc upload nhất quán.

namespace MediaService.Domain.Constants;

public static class MediaTypes
{
    public const string Image = "IMAGE";
    public const string Video = "VIDEO";
    public const string Document = "DOCUMENT";
    public const string Audio = "AUDIO";
    public const string Other = "OTHER";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal)
        {
            Image, Video, Document, Audio, Other,
        };
}
