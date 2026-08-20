// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/Create/CreateMediaUsageHandler.cs
// Mục đích: Điều phối use case CreateMediaUsageHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Application.Services.Students;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.MediaUsages.Create;

public sealed class CreateMediaUsageHandler(
    IActorValidationService actorValidationService,
    IStudentLookup studentLookup,
    IMediaRepository mediaRepository,
    IMediaUsageRepository mediaUsageRepository)
{
    public async Task<CreateMediaUsageResult> HandleAsync(
        CreateMediaUsageCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var ownerService = command.OwnerService?.Trim().ToUpperInvariant();
        var ownerType = command.OwnerType?.Trim().ToUpperInvariant();
        var usageType = command.UsageType?.Trim().ToUpperInvariant();
        ValidateCommand(command, ownerService, ownerType, usageType);
        var isStudentAvatar = ownerService == MediaOwnerServices.Student;
        var isCourseThumbnail = ownerService == MediaOwnerServices.Course &&
            ownerType == MediaOwnerTypes.CourseThumbnail;
        var isCourseGallery = ownerService == MediaOwnerServices.Course &&
            ownerType == MediaOwnerTypes.CourseGallery;
        var actor = await actorValidationService.ValidateAsync(
            command.CreatedBy,
            cancellationToken);
        if (ownerService == MediaOwnerServices.Course && actor.Type != ActorTypes.Admin)
        {
            throw MediaErrors.InvalidActorType("Only ADMIN actors can manage Course media.");
        }

        if (isStudentAvatar &&
            (actor.Type != ActorTypes.Student || actor.Id != command.OwnerId))
        {
            var owner = await studentLookup.GetByIdAsync(
                command.OwnerId,
                cancellationToken);
            if (owner is null)
            {
                throw MediaErrors.OwnerNotFound();
            }
        }

        var media = await mediaRepository.GetByIdAsync(
            command.MediaId,
            cancellationToken);
        if (media is null)
        {
            throw MediaErrors.MediaNotFound();
        }

        if (media.Status != MediaObjectStatuses.Ready ||
            media.DeletedAtUtc is not null)
        {
            throw MediaErrors.MediaNotReady();
        }

        if (isCourseGallery &&
            (media.MediaType != MediaTypes.Image || media.SourceMediaId is not null))
        {
            throw MediaErrors.InvalidMedia("Course gallery media must be a READY original image.");
        }

        if (!isStudentAvatar && !isCourseGallery &&
            (!string.Equals(media.DerivationType, MediaDerivationTypes.Thumbnail, StringComparison.Ordinal) ||
             !string.Equals(media.ContentType, "image/webp", StringComparison.OrdinalIgnoreCase)))
        {
            throw MediaErrors.InvalidMedia(
                "mediaId must reference a READY WebP thumbnail.");
        }

        var record = new CreateMediaUsageRecord(
            Guid.NewGuid(),
            command.MediaId,
            ownerService!,
            ownerType!,
            command.OwnerId,
            usageType!,
            command.DisplayOrder,
            actor);
        var usage = isStudentAvatar
            ? await mediaUsageRepository.ReplaceStudentAvatarAsync(record, cancellationToken)
            : isCourseThumbnail
                ? await mediaUsageRepository.ReplaceCourseThumbnailAsync(record, cancellationToken)
                : isCourseGallery
                    ? await mediaUsageRepository.AddCourseGalleryMediaAsync(record, cancellationToken)
                    : await mediaUsageRepository.ReplaceMediaThumbnailAsync(record, cancellationToken);

        return new CreateMediaUsageResult(
            usage.Id,
            usage.MediaId,
            usage.OwnerService,
            usage.OwnerType,
            usage.OwnerId,
            usage.UsageType,
            usage.DisplayOrder,
            usage.CreatedAtUtc);
    }

    private static void ValidateCommand(
        CreateMediaUsageCommand command,
        string? ownerService,
        string? ownerType,
        string? usageType)
    {
        if (command.MediaId == Guid.Empty || command.OwnerId == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia(
                "mediaId and ownerId must be valid UUIDs.");
        }

        var isStudentAvatar =
            ownerService == MediaOwnerServices.Student &&
            ownerType == MediaOwnerTypes.StudentAvatar &&
            usageType == MediaUsageTypes.Avatar;
        var isMediaThumbnail =
            ownerService == MediaOwnerServices.Media &&
            ownerType == MediaOwnerTypes.MediaThumbnail &&
            usageType == MediaUsageTypes.Thumbnail;
        var isCourseThumbnail =
            ownerService == MediaOwnerServices.Course &&
            ownerType == MediaOwnerTypes.CourseThumbnail &&
            usageType == MediaUsageTypes.Thumbnail;
        var isCourseGallery =
            ownerService == MediaOwnerServices.Course &&
            ownerType == MediaOwnerTypes.CourseGallery &&
            usageType == MediaUsageTypes.Attachment;
        if (!isStudentAvatar && !isMediaThumbnail && !isCourseThumbnail && !isCourseGallery)
        {
            throw MediaErrors.InvalidMedia(
                "Supported usages are STUDENT/STUDENT_AVATAR/AVATAR, MEDIA/MEDIA_THUMBNAIL/THUMBNAIL, " +
                "and COURSE/COURSE_THUMBNAIL/THUMBNAIL or COURSE/COURSE_GALLERY/ATTACHMENT.");
        }
    }
}
