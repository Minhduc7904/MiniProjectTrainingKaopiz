// File: backend/Services/Course/CourseService.Application/UseCases/Courses/GetList/GetCoursesHandler.cs
// Mục đích: Điều phối use case lấy Course theo phân trang, không dùng đường tải toàn bộ.

using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Courses.GetList;

public sealed class GetCoursesHandler(ICourseListRepository courseListRepository)
{
    public Task<GetCoursesResult> HandleAsync(GetCoursesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return courseListRepository.GetPagedAsync(query, cancellationToken);
    }
}
