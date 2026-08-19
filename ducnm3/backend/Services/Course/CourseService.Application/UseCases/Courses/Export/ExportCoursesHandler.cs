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
        ArgumentNullException.ThrowIfNull(query);
        return courseListRepository.ReadExportChunkAsync(query, position, cancellationToken);
    }
}
