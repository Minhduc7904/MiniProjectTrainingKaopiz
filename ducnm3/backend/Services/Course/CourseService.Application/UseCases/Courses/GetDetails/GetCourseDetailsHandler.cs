using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Courses.GetDetails;

public sealed class GetCourseDetailsHandler(ICourseDetailsRepository repository)
{
    public async Task<CourseDetailsResult> HandleAsync(
        Guid courseId,
        CancellationToken cancellationToken) =>
        await repository.GetWithoutNPlusOneAsync(courseId, cancellationToken)
        ?? throw CourseErrors.CourseNotFound();
}
