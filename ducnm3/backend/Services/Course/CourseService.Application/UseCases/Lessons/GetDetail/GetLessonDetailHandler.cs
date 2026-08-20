using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Lessons.GetDetail;

public sealed class GetLessonDetailHandler(ILessonCommandRepository repository)
{
    public async Task<LessonCreateRecord> HandleAsync(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        return await repository.GetAsync(courseId, lessonId, cancellationToken)
            ?? throw CourseErrors.LessonNotFound();
    }
}
