using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Lessons.Delete;

public sealed class DeleteLessonHandler(
    ILessonCommandRepository repository,
    ICourseMediaReader mediaReader,
    ICommandSender commandSender)
{
    public async Task HandleAsync(Guid courseId, Guid lessonId, Guid actorId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty || actorId == Guid.Empty)
            throw CourseErrors.ValidationFailed([]);

        _ = await repository.GetAsync(courseId, lessonId, cancellationToken)
            ?? throw CourseErrors.LessonNotFound();
        var usageIds = await mediaReader.GetActiveUsageIdsAsync(
            CourseMediaUsageScopes.ForLesson(lessonId), cancellationToken);
        if (!await repository.DeleteAsync(courseId, lessonId, cancellationToken))
            throw CourseErrors.LessonNotFound();
        if (usageIds.Count > 0)
            await commandSender.SendAsync(ServiceNames.Media,
                new DeleteMediaUsagesByIdsV1(usageIds.Distinct().ToArray()), cancellationToken);
    }
}
