// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/CourseExportChunk.cs
// Mục đích: Giới hạn dữ liệu repository trả về một chunk cho mỗi lần đọc export.

namespace CourseService.Application.UseCases.Courses.Export;

public sealed record CourseExportChunk
{
    public CourseExportChunk(IReadOnlyList<CourseExportRow> rows, int readCountBefore = 0)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentOutOfRangeException.ThrowIfNegative(readCountBefore);
        if (rows.Count > ExportCoursesQuery.ChunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), $"An export chunk cannot contain more than {ExportCoursesQuery.ChunkSize} rows.");
        }

        Rows = rows;
        ReadCountBefore = readCountBefore;
    }

    public IReadOnlyList<CourseExportRow> Rows { get; }
    public int ReadCountBefore { get; }

    public CourseExportPosition? NextPosition => Rows.Count == 0
        ? null
        : new CourseExportPosition(
            Rows[^1].CreatedAtUtc,
            Rows[^1].Id,
            ReadCountBefore + Rows.Count);
}
