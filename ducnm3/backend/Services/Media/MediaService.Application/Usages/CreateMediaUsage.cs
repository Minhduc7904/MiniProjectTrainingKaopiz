using MediaService.Application.Actors;
using MediaService.Application.Persistence;
using MediaService.Domain;

namespace MediaService.Application.Usages;

public sealed record CreateMediaUsageCommand(
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    ActorReference CreatedBy);

public sealed record CreateMediaUsageResult(
    Guid Id,
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    DateTime CreatedAtUtc);

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
        ValidateCommand(command);
        var actor = await actorValidationService.ValidateAsync(
            command.CreatedBy,
            cancellationToken);

        if (actor.Type != ActorTypes.Student || actor.Id != command.OwnerId)
        {
            var owner = await studentLookup.GetByIdAsync(
                command.OwnerId,
                cancellationToken);
            if (owner is null)
            {
                throw new MediaApplicationException(
                    MediaErrorCodes.OwnerNotFound,
                    "The media usage owner does not exist.",
                    404);
            }
        }

        var media = await mediaRepository.GetByIdAsync(
            command.MediaId,
            cancellationToken);
        if (media is null)
        {
            throw new MediaApplicationException(
                MediaErrorCodes.MediaNotFound,
                "Media was not found.",
                404);
        }

        if (media.Status != MediaObjectStatuses.Ready ||
            media.DeletedAtUtc is not null)
        {
            throw new MediaApplicationException(
                MediaErrorCodes.MediaNotReady,
                "Media is not ready for use.",
                409);
        }

        var usage = await mediaRepository.ReplaceStudentAvatarAsync(
            new CreateMediaUsageRecord(
                Guid.NewGuid(),
                command.MediaId,
                MediaOwnerServices.Student,
                MediaOwnerTypes.StudentAvatar,
                command.OwnerId,
                MediaUsageTypes.Avatar,
                command.DisplayOrder,
                actor),
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

    private static void ValidateCommand(CreateMediaUsageCommand command)
    {
        if (command.MediaId == Guid.Empty || command.OwnerId == Guid.Empty)
        {
            throw InvalidUsage("mediaId and ownerId must be valid UUIDs.");
        }

        if (!string.Equals(
                command.OwnerService?.Trim(),
                MediaOwnerServices.Student,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                command.OwnerType?.Trim(),
                MediaOwnerTypes.StudentAvatar,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                command.UsageType?.Trim(),
                MediaUsageTypes.Avatar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw InvalidUsage(
                "Only STUDENT/STUDENT_AVATAR/AVATAR usage is supported.");
        }
    }

    private static MediaApplicationException InvalidUsage(string message) =>
        new(MediaErrorCodes.InvalidMedia, message, 400);
}
