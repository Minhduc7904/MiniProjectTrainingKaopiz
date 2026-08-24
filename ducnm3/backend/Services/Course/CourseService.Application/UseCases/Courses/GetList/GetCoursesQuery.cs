// File: backend/Services/Course/CourseService.Application/UseCases/Courses/GetList/GetCoursesQuery.cs
// Mục đích: Normalize và validate filter, sort, offset pagination trước khi query Course.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.Courses.GetList;

public sealed record GetCoursesQuery
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;

    private GetCoursesQuery(string? status, string? search, CourseSortField sortBy, bool descending, int page, int pageSize) =>
        (Status, Search, SortBy, Descending, Page, PageSize) = (status, search, sortBy, descending, page, pageSize);

    public string? Status { get; }
    public string? Search { get; }
    public CourseSortField SortBy { get; }
    public bool Descending { get; }
    public int Page { get; }
    public int PageSize { get; }

    public static GetCoursesQuery Create(string? status, string? search, string? sortBy, string? sortDirection, int? page, int? pageSize)
    {
        var details = new List<ApiErrorDetail>();
        var normalizedPage = page ?? DefaultPage;
        var normalizedPageSize = pageSize ?? DefaultPageSize;
        var normalizedStatus = NormalizeStatus(status, details);
        var normalizedSearch = NormalizeSearch(search);
        var normalizedSort = NormalizeSort(sortBy, details);
        var descending = NormalizeDirection(sortDirection, details);
        if (normalizedPage < 1) details.Add(new ApiErrorDetail("page", "Page must be greater than or equal to 1."));
        if (normalizedPageSize is < 1 or > MaximumPageSize) details.Add(new ApiErrorDetail("pageSize", $"Page size must be between 1 and {MaximumPageSize}."));
        if (((long)normalizedPage - 1) * normalizedPageSize > int.MaxValue) details.Add(new ApiErrorDetail("page", "The requested page is outside the supported range."));
        if (details.Count > 0) throw CourseErrors.ValidationFailed(details);
        return new GetCoursesQuery(normalizedStatus, normalizedSearch, normalizedSort, descending, normalizedPage, normalizedPageSize);
    }

    private static string? NormalizeStatus(string? status, List<ApiErrorDetail> details)
    {
        if (string.IsNullOrWhiteSpace(status)) return null;
        var normalized = status.Trim().ToUpperInvariant();
        if (CourseStatuses.IsSupported(normalized)) return normalized;
        details.Add(new ApiErrorDetail("status", $"Status must be {CourseStatuses.Draft}, {CourseStatuses.Published}, or {CourseStatuses.Archived}."));
        return null;
    }

    private static string? NormalizeSearch(string? search) =>
        string.IsNullOrWhiteSpace(search) ? null : search.Trim();

    private static CourseSortField NormalizeSort(string? sortBy, List<ApiErrorDetail> details) =>
        string.IsNullOrWhiteSpace(sortBy) ? CourseSortField.CreatedAt : sortBy.Trim().ToLowerInvariant() switch
        {
            "createdat" => CourseSortField.CreatedAt,
            "name" => CourseSortField.Name,
            _ => InvalidSort(details),
        };

    private static CourseSortField InvalidSort(List<ApiErrorDetail> details)
    {
        details.Add(new ApiErrorDetail("sortBy", "Sort field must be createdAt or name."));
        return CourseSortField.CreatedAt;
    }

    private static bool NormalizeDirection(string? direction, List<ApiErrorDetail> details) =>
        string.IsNullOrWhiteSpace(direction) ? true : direction.Trim().ToLowerInvariant() switch
        {
            "asc" => false,
            "desc" => true,
            _ => InvalidDirection(details),
        };

    private static bool InvalidDirection(List<ApiErrorDetail> details)
    {
        details.Add(new ApiErrorDetail("sortDirection", "Sort direction must be asc or desc."));
        return true;
    }
}
