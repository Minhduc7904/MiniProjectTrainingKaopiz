// File: backend/Services/Media/MediaService.Application/Services/MediaUsages/MediaUsageAssignmentPolicy.cs
// Mục đích: Tập trung policy cho phép gán media theo owner, usage, actor và thuộc tính media.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.Services.MediaUsages;

public static class MediaUsageAssignmentPolicy
{
    public static MediaUsageAssignmentKind ValidateAndClassify(
        string? ownerService,
        string? ownerType,
        string? usageType,
        Guid ownerId,
        ActorReference actor,
        MediaRecord media)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(media);

        if (ownerId == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia("ownerId must be a valid UUID.");
        }

        EnsureMediaIsReady(media);
        return (ownerService, ownerType, usageType) switch
        {
            (MediaOwnerServices.Student, MediaOwnerTypes.StudentAvatar, MediaUsageTypes.Avatar) =>
                ValidateStudentAvatar(ownerId, actor, media),
            (MediaOwnerServices.Media, MediaOwnerTypes.MediaThumbnail, MediaUsageTypes.Thumbnail) =>
                ValidateMediaThumbnail(ownerId, actor, media),
            (MediaOwnerServices.Course, MediaOwnerTypes.CourseThumbnail, MediaUsageTypes.Thumbnail) =>
                ValidateCourseThumbnail(actor, media),
            (MediaOwnerServices.Course, MediaOwnerTypes.CourseGallery, MediaUsageTypes.Attachment) =>
                ValidateCourseGallery(actor, media),
            (MediaOwnerServices.Course, MediaOwnerTypes.LessonAttachment, MediaUsageTypes.Attachment) =>
                ValidateLessonAttachment(actor, media),
            _ => throw MediaErrors.InvalidMedia(
                "Supported direct usages are STUDENT/STUDENT_AVATAR/AVATAR, " +
                "MEDIA/MEDIA_THUMBNAIL/THUMBNAIL, COURSE/COURSE_THUMBNAIL/THUMBNAIL, " +
                "COURSE/COURSE_GALLERY/ATTACHMENT, or COURSE/LESSON_ATTACHMENT/ATTACHMENT."),
        };
    }

    private static MediaUsageAssignmentKind ValidateStudentAvatar(
        Guid ownerId,
        ActorReference actor,
        MediaRecord media)
    {
        if (actor.Type != ActorTypes.Student || actor.Id != ownerId)
        {
            throw MediaErrors.InvalidActorType("A Student can manage only their own avatar.");
        }

        EnsureReadyOriginalImage(media, "Student avatar media");
        return MediaUsageAssignmentKind.StudentAvatar;
    }

    private static MediaUsageAssignmentKind ValidateMediaThumbnail(
        Guid ownerId,
        ActorReference actor,
        MediaRecord media)
    {
        EnsureAdmin(actor, "Only ADMIN actors can manage media thumbnails.");
        if (media.SourceMediaId != ownerId ||
            media.DerivationType != MediaDerivationTypes.Thumbnail ||
            !string.Equals(media.ContentType, "image/webp", StringComparison.OrdinalIgnoreCase))
        {
            throw MediaErrors.InvalidMedia(
                "Media thumbnail must be a READY WebP thumbnail derived from the owner media.");
        }

        return MediaUsageAssignmentKind.MediaThumbnail;
    }

    private static MediaUsageAssignmentKind ValidateCourseThumbnail(
        ActorReference actor,
        MediaRecord media)
    {
        EnsureAdmin(actor, "Only ADMIN actors can manage Course media.");
        EnsureReadyOriginalImage(media, "Course thumbnail media");
        return MediaUsageAssignmentKind.CourseThumbnail;
    }

    private static MediaUsageAssignmentKind ValidateCourseGallery(
        ActorReference actor,
        MediaRecord media)
    {
        EnsureAdmin(actor, "Only ADMIN actors can manage Course media.");
        EnsureReadyOriginalImage(media, "Course gallery media");
        return MediaUsageAssignmentKind.CourseGallery;
    }

    private static MediaUsageAssignmentKind ValidateLessonAttachment(
        ActorReference actor,
        MediaRecord media)
    {
        EnsureAdmin(actor, "Only ADMIN actors can manage Course media.");
        if (media.SourceMediaId is not null)
        {
            throw MediaErrors.InvalidMedia("Lesson attachment media must be an original media object.");
        }

        return MediaUsageAssignmentKind.LessonAttachment;
    }

    private static void EnsureMediaIsReady(MediaRecord media)
    {
        if (media.Status != MediaObjectStatuses.Ready || media.DeletedAtUtc is not null)
        {
            throw MediaErrors.MediaNotReady();
        }
    }

    private static void EnsureReadyOriginalImage(MediaRecord media, string mediaName)
    {
        if (media.MediaType != MediaTypes.Image || media.SourceMediaId is not null)
        {
            throw MediaErrors.InvalidMedia($"{mediaName} must be a READY original image.");
        }
    }

    private static void EnsureAdmin(ActorReference actor, string message)
    {
        if (actor.Type != ActorTypes.Admin)
        {
            throw MediaErrors.InvalidActorType(message);
        }
    }
}

public enum MediaUsageAssignmentKind
{
    StudentAvatar,
    MediaThumbnail,
    CourseThumbnail,
    CourseGallery,
    LessonAttachment,
}
