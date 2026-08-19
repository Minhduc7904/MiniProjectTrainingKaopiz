// File: backend/Services/Course/CourseService.Application/Repositories/ICourseListRepository.cs
// Mục đích: Khai báo hai đường đọc Course độc lập để so sánh benchmark và phục vụ phân trang.

using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetList;

namespace CourseService.Application.Repositories;

public interface ICourseListRepository
{
    Task<IReadOnlyList<CourseListItemRecord>> GetAllAsync(CancellationToken cancellationToken);

    Task<GetCoursesResult> GetPagedAsync(
        GetCoursesQuery query,
        CancellationToken cancellationToken);

    Task<CourseExportChunk> ReadExportChunkAsync(
        ExportCoursesQuery query,
        CourseExportPosition? position,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseExportRow>> ReadAllExportRowsAsync(
        ExportCoursesQuery query,
        CancellationToken cancellationToken);

}
