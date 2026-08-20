// File: backend/Services/Media/MediaService.Domain/Constants/MediaOwnerTypes.cs
// Mục đích: Khai báo các loại owner được phép gắn Media Usage, phục vụ validate ownership.

namespace MediaService.Domain.Constants;

public static class MediaOwnerTypes
{
    public const string CourseDescription = "COURSE_DESCRIPTION";
    public const string CourseGallery = "COURSE_GALLERY";
    public const string CourseThumbnail = "COURSE_THUMBNAIL";
    public const string MediaThumbnail = "MEDIA_THUMBNAIL";
    public const string NotificationBody = "NOTIFICATION_BODY";
    public const string LessonContent = "LESSON_CONTENT";
    public const string LessonAttachment = "LESSON_ATTACHMENT";
    public const string StudentAvatar = "STUDENT_AVATAR";
}
