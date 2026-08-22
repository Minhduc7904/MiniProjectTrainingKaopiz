using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Courses.GetSummary;

public sealed class GetCoursesSummaryHandler(ICourseSummaryRepository repository)
{
    public async Task<CoursesSummaryResult> HandleAsync(CancellationToken cancellationToken)
    {
        var (totalCourses, totalLessons) = await repository.CountAsync(cancellationToken);
        return new CoursesSummaryResult(totalCourses, totalLessons);
    }
}

public sealed record CoursesSummaryResult(long TotalCourses, long TotalLessons);
