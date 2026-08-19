// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/ExportCoursesQuery.cs
// Mục đích: Normalize và validate filter cho luồng xuất Course.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.UseCases.Courses.GetList;

namespace CourseService.Application.UseCases.Courses.Export;

public sealed record ExportCoursesQuery
{
    public const int ChunkSize = 500;

    private ExportCoursesQuery(string? status) => Status = status;

    public string? Status { get; }

    public static ExportCoursesQuery Create(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return new ExportCoursesQuery((string?)null);

        var normalizedStatus = status.Trim().ToUpperInvariant();
        if (normalizedStatus is "DRAFT" or "PUBLISHED" or "ARCHIVED")
        {
            return new ExportCoursesQuery(normalizedStatus);
        }

        throw new CourseListValidationException(
            [new ApiErrorDetail("status", "Status must be DRAFT, PUBLISHED, or ARCHIVED.")]);
    }
}
