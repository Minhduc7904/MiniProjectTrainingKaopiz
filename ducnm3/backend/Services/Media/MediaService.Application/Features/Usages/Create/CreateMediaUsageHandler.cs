using MediaService.Application.Abstractions.Clients;
using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Actors;
using MediaService.Domain.Actors;
using MediaService.Domain.Media;
using MediaService.Domain.Usages;

namespace MediaService.Application.Features.Usages.Create;

public sealed class CreateMediaUsageHandler(
    IActorValidationService actorValidationService,
    IStudentLookup studentLookup,
    IMediaRepository mediaRepository)
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
        var isStudentAvatar =
            ownerService == MediaOwnerServices.Student;
        var actor = await actorValidationService.ValidateAsync(
            command.CreatedBy,
            cancellationToken);

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

        if (!isStudentAvatar &&
            (!string.Equals(
                 media.DerivationType,
                 MediaDerivationTypes.Thumbnail,
                 StringComparison.Ordinal) ||
             !string.Equals(
                 media.ContentType,
                 "image/webp",
                 StringComparison.OrdinalIgnoreCase)))
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
            ? await mediaRepository.ReplaceStudentAvatarAsync(
                record,
                cancellationToken)
            : await mediaRepository.ReplaceMediaThumbnailAsync(
                record,
                cancellationToken);

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
        if (!isStudentAvatar && !isMediaThumbnail)
        {
            throw MediaErrors.InvalidMedia(
                "Supported usages are STUDENT/STUDENT_AVATAR/AVATAR and " +
                "MEDIA/MEDIA_THUMBNAIL/THUMBNAIL.");
        }
    }
}
