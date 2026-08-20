using MediaService.Application.Repositories;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.MediaUsages.RegisterCourseLesson;

public sealed class RegisterCourseLessonMediaUsagesHandler(IMediaUsageRepository repository)
{
    public Task HandleAsync(
        RegisterCourseLessonMediaUsageV1 command,
        CancellationToken cancellationToken)
    {
        if (command.LessonId == Guid.Empty || command.CreatedBy == Guid.Empty || command.References.Count == 0)
        {
            throw new ArgumentException("Lesson media usage command is invalid.");
        }

        var usages = command.References.Select(reference => new CreateMediaUsageRecord(
            Guid.NewGuid(),
            reference.MediaId,
            MediaOwnerServices.Course,
            MediaOwnerTypes.LessonContent,
            command.LessonId,
            reference.UsageType,
            reference.DisplayOrder,
            new ActorReference(ActorTypes.Admin, command.CreatedBy))).ToArray();
        return repository.EnsureCourseLessonMediaAsync(usages, cancellationToken);
    }
}
