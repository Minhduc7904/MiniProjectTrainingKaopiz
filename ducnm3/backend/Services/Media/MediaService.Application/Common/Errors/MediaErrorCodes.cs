// File: backend/Services/Media/MediaService.Application/Common/Errors/MediaErrorCodes.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Common.Errors;

public static class MediaErrorCodes
{
    public const string InvalidMedia = "INVALID_MEDIA";
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";
    public const string MediaUploadFailed = "MEDIA_UPLOAD_FAILED";
    public const string DirectUploadIncomplete = "MEDIA_UPLOAD_INCOMPLETE";
    public const string MediaNotFound = "MEDIA_NOT_FOUND";
    public const string MediaNotReady = "MEDIA_NOT_READY";
    public const string MediaUsageConflict = "MEDIA_USAGE_CONFLICT";
    public const string MediaUsageNotFound = "MEDIA_USAGE_NOT_FOUND";
    public const string ThumbnailNotFound = "MEDIA_THUMBNAIL_NOT_FOUND";
    public const string ThumbnailRetryConflict = "MEDIA_THUMBNAIL_RETRY_CONFLICT";
    public const string InvalidActorType = "INVALID_ACTOR_TYPE";
    public const string ActorNotFound = "ACTOR_NOT_FOUND";
    public const string OwnerNotFound = "OWNER_NOT_FOUND";
    public const string StudentServiceUnavailable = "STUDENT_SERVICE_UNAVAILABLE";
}
