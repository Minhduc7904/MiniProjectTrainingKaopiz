namespace CourseService.Application.Repositories;

public interface ICourseSummaryRepository
{
    Task<(long TotalCourses, long TotalLessons)> CountAsync(CancellationToken cancellationToken);
}
