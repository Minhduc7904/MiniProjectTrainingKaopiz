using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.MediaUsages.SynchronizeCourseContent;

public sealed class SynchronizeCourseContentMediaUsagesHandler(IMediaUsageRepository repository)
{
    public async Task HandleAsync(
        SynchronizeCourseContentMediaUsageV1 command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.OwnerId == Guid.Empty || command.CreatedBy == Guid.Empty ||
            command.OwnerType is not (MediaOwnerTypes.CourseDescription or MediaOwnerTypes.LessonContent) ||
            command.Added.Concat(command.Removed).Any(reference =>
                reference.MediaId == Guid.Empty ||
                reference.UsageType is not (NotificationMediaUsageTypes.Embed or NotificationMediaUsageTypes.Attachment)))
        {
            throw MediaErrors.InvalidMedia("Course content media usage command is invalid.");
        }

        var actor = new ActorReference(ActorTypes.Admin, command.CreatedBy);
        var additions = command.Added
            .GroupBy(reference => (reference.MediaId, reference.UsageType))
            .Select(group => group.First())
            .Select(reference => new CreateMediaUsageRecord(
                Guid.NewGuid(),
                reference.MediaId,
                MediaOwnerServices.Course,
                command.OwnerType,
                command.OwnerId,
                reference.UsageType,
                reference.DisplayOrder,
                actor))
            .ToArray();
        if (additions.Length > 0)
        {
            await repository.EnsureCourseLessonMediaAsync(additions, cancellationToken);
        }

        var removals = command.Removed
            .GroupBy(reference => (reference.MediaId, reference.UsageType))
            .Select(group => group.First())
            .Select(reference => new CourseContentMediaUsageRemoval(
                command.OwnerId,
                command.OwnerType,
                reference.MediaId,
                reference.UsageType))
            .ToArray();
        if (removals.Length > 0)
        {
            await repository.RemoveCourseContentMediaAsync(removals, cancellationToken);
        }
    }
}
