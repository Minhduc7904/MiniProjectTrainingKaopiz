// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/ExportCoursesQuery.cs
// Mục đích: Normalize và validate filter cho luồng xuất Course.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.Courses.Export;

public sealed record ExportCoursesQuery
{
    // Giới hạn bộ nhớ/database work của một lần đọc; Endpoint sẽ lặp nhiều lần nếu file lớn hơn 500 Course.
    public const int ChunkSize = 500;

    private ExportCoursesQuery(string? status, int? limit)
    {
        Status = status;
        Limit = limit;
    }

    public string? Status { get; }
    public int? Limit { get; }

    public static ExportCoursesQuery Create(string? status, int? limit = null)
    {
        // limit null nghĩa là không giới hạn tổng số row; giá trị dương là quota cho toàn bộ file, không phải mỗi chunk.
        if (limit is <= 0)
        {
            throw CourseErrors.ValidationFailed(
                [new ApiErrorDetail("limit", "Limit must be greater than zero.")]);
        }

        // Filter trống không thêm WHERE status; có filter thì normalize để so sánh với domain constant trong DB.
        if (string.IsNullOrWhiteSpace(status)) return new ExportCoursesQuery(null, limit);

        var normalizedStatus = status.Trim().ToUpperInvariant();
        if (CourseStatuses.IsSupported(normalizedStatus))
        {
            return new ExportCoursesQuery(normalizedStatus, limit);
        }

        throw CourseErrors.ValidationFailed(
            [new ApiErrorDetail("status", $"Status must be {CourseStatuses.Draft}, {CourseStatuses.Published}, or {CourseStatuses.Archived}.")]);
    }
}
