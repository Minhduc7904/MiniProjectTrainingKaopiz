using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Courses.Delete;

public sealed class DeleteCourseHandler(
    ICourseCommandRepository repository,
    ICourseMediaReader mediaReader,
    ICommandSender commandSender)
{
    public async Task HandleAsync(
        Guid courseId,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || actorId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        var course = await repository.GetAsync(courseId, cancellationToken)
            ?? throw CourseErrors.CourseNotFound();
        var lessonIds = await repository.GetLessonIdsAsync(course.Id, cancellationToken);
        var usageIds = await mediaReader.GetActiveUsageIdsAsync(
            CourseMediaUsageScopes.ForCourse(course.Id, lessonIds),
            cancellationToken);

        var deleted = await repository.DeleteAsync(course.Id, cancellationToken);
        if (!deleted)
        {
            throw CourseErrors.CourseNotFound();
        }

        if (usageIds.Count > 0)
        {
            await commandSender.SendAsync(
                ServiceNames.Media,
                new DeleteMediaUsagesByIdsV1(usageIds.Distinct().ToArray()),
                cancellationToken);
        }
    }
}
