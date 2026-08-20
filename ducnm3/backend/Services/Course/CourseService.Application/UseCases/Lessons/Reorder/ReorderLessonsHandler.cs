using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Lessons.Reorder;

public sealed class ReorderLessonsHandler(ILessonCommandRepository repository)
{
    public async Task HandleAsync(Guid courseId, IReadOnlyList<Guid> lessonIds, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonIds.Count == 0 || lessonIds.Any(id => id == Guid.Empty)) throw CourseErrors.ValidationFailed([]);
        if (!await repository.ReorderAsync(courseId, lessonIds, cancellationToken)) throw CourseErrors.LessonOrderConflict();
    }
}
