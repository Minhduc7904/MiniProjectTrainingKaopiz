// File: backend/Services/Media/MediaService.Domain/Constants/MediaOwnerServices.cs
// Mục đích: Khai báo service owner hợp lệ có thể tham chiếu Media khi tạo Media Usage.

namespace MediaService.Domain.Constants;

public static class MediaOwnerServices
{
    public const string Course = "COURSE";
    public const string Media = "MEDIA";
    public const string Notification = "NOTIFICATION";
    public const string Student = "STUDENT";
}
