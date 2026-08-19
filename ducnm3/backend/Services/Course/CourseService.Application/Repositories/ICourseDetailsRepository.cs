using CourseService.Application.UseCases.Courses.GetDetails;

namespace CourseService.Application.Repositories;

public interface ICourseDetailsRepository
{
    Task<CourseDetailsResult?> GetWithoutNPlusOneAsync(
        Guid courseId,
        CancellationToken cancellationToken);

    Task<CourseDetailsResult?> GetWithNPlusOneAsync(
        Guid courseId,
        CancellationToken cancellationToken);
}
