// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/ExportCoursesQuery.cs
// Mục đích: Normalize và validate filter cho luồng xuất Course.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.Courses.Export;

public sealed record ExportCoursesQuery
{
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
        if (limit is <= 0)
        {
            throw CourseErrors.ValidationFailed(
                [new ApiErrorDetail("limit", "Limit must be greater than zero.")]);
        }

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
