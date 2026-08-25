// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/CourseExportChunk.cs
// Mục đích: Giới hạn dữ liệu repository trả về một chunk cho mỗi lần đọc export.

namespace CourseService.Application.UseCases.Courses.Export;

public sealed record CourseExportChunk
{
    public CourseExportChunk(IReadOnlyList<CourseExportRow> rows, int readCountBefore = 0)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentOutOfRangeException.ThrowIfNegative(readCountBefore);
        // Bảo vệ invariant giữa Infrastructure và Endpoint: một chunk không được âm thầm vượt giới hạn memory đã chọn.
        if (rows.Count > ExportCoursesQuery.ChunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), $"An export chunk cannot contain more than {ExportCoursesQuery.ChunkSize} rows.");
        }

        Rows = rows;
        ReadCountBefore = readCountBefore;
    }

    public IReadOnlyList<CourseExportRow> Rows { get; }
    public int ReadCountBefore { get; }

    // Cursor vòng sau luôn lấy row cuối của thứ tự DESC. ReadCount mới là tổng row đã xuất, không chỉ row của chunk này.
    public CourseExportPosition? NextPosition => Rows.Count == 0
        ? null
        : new CourseExportPosition(
            Rows[^1].CreatedAtUtc,
            Rows[^1].Id,
            ReadCountBefore + Rows.Count);
}
