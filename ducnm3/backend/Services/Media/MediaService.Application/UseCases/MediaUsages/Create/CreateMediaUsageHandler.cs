// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/Create/CreateMediaUsageHandler.cs
// Mục đích: Điều phối use case CreateMediaUsageHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Application.Services.MediaUsages;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.MediaUsages.Create;

public sealed class CreateMediaUsageHandler(
    IActorValidationService actorValidationService,
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
        var actor = await actorValidationService.ValidateAsync(
            command.CreatedBy,
            cancellationToken);

        var media = await mediaRepository.GetByIdAsync(
            command.MediaId,
            cancellationToken);
        if (media is null)
        {
            throw MediaErrors.MediaNotFound();
        }

        var assignmentKind = MediaUsageAssignmentPolicy.ValidateAndClassify(
            ownerService,
            ownerType,
            usageType,
            command.OwnerId,
            actor,
            media);

        var record = new CreateMediaUsageRecord(
            Guid.NewGuid(),
            command.MediaId,
            ownerService!,
            ownerType!,
            command.OwnerId,
            usageType!,
            command.DisplayOrder,
            actor);
        var usage = assignmentKind switch
        {
            MediaUsageAssignmentKind.StudentAvatar =>
                await mediaUsageRepository.ReplaceStudentAvatarAsync(record, cancellationToken),
            MediaUsageAssignmentKind.CourseThumbnail =>
                await mediaUsageRepository.ReplaceCourseThumbnailAsync(record, cancellationToken),
            MediaUsageAssignmentKind.CourseGallery =>
                await mediaUsageRepository.AddCourseGalleryMediaAsync(record, cancellationToken),
            MediaUsageAssignmentKind.LessonAttachment =>
                await AddLessonAttachmentAsync(record, cancellationToken),
            _ => throw new InvalidOperationException("Unsupported media usage assignment."),
        };

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

        if (string.IsNullOrWhiteSpace(ownerService) ||
            string.IsNullOrWhiteSpace(ownerType) ||
            string.IsNullOrWhiteSpace(usageType))
        {
            throw MediaErrors.InvalidMedia("ownerService, ownerType and usageType are required.");
        }
    }

    private async Task<MediaService.Domain.Entities.MediaUsage> AddLessonAttachmentAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
    {
        await mediaUsageRepository.EnsureCourseLessonMediaAsync([usage], cancellationToken);
        return new MediaService.Domain.Entities.MediaUsage(
            usage.Id, usage.MediaId, usage.OwnerService, usage.OwnerType, usage.OwnerId,
            usage.UsageType, usage.DisplayOrder, usage.CreatedBy, DateTime.UtcNow);
    }
}
