// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/ExportCoursesQuery.cs
// Mục đích: Normalize và validate filter cho luồng xuất Course.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Domain.Constants;

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
        if (CourseStatuses.IsSupported(normalizedStatus))
        {
            return new ExportCoursesQuery(normalizedStatus);
        }

        throw CourseErrors.ValidationFailed(
            [new ApiErrorDetail("status", $"Status must be {CourseStatuses.Draft}, {CourseStatuses.Published}, or {CourseStatuses.Archived}.")]);
    }
}
