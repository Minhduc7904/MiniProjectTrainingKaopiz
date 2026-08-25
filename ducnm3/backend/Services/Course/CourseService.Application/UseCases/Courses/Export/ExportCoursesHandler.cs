// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/ExportCoursesHandler.cs
// Mục đích: Điều phối đọc từng chunk Course cho luồng CSV export.

using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Courses.Export;

public sealed class ExportCoursesHandler(ICourseListRepository courseListRepository)
{
    public Task<CourseExportChunk> HandleAsync(
        ExportCoursesQuery query,
        CourseExportPosition? position,
        CancellationToken cancellationToken)
    {
        // Application chỉ điều phối use case: không biết HTTP response, CSV bytes hay EF Core.
        // position được Endpoint chuyển lại giữa các vòng để repository đọc trang keyset kế tiếp.
        ArgumentNullException.ThrowIfNull(query);
        return courseListRepository.ReadExportChunkAsync(query, position, cancellationToken);
    }
}
