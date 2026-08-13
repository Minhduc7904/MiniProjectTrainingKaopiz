using BuildingBlocks.Contracts.Api;

namespace StudentService.Application.Features.Students.GetList;

public sealed record GetStudentsQuery
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;

    private GetStudentsQuery(
        string? status,
        StudentSortField sortBy,
        bool descending,
        int page,
        int pageSize)
    {
        Status = status;
        SortBy = sortBy;
        Descending = descending;
        Page = page;
        PageSize = pageSize;
    }

    public string? Status { get; }

    public StudentSortField SortBy { get; }

    public bool Descending { get; }

    public int Page { get; }

    public int PageSize { get; }

    public static GetStudentsQuery Create(
        string? status,
        string? sortBy,
        string? sortDirection,
        int? page,
        int? pageSize)
    {
        var details = new List<ApiErrorDetail>();
        var normalizedStatus = NormalizeStatus(status, details);
        var normalizedSort = NormalizeSort(sortBy, details);
        var descending = NormalizeDirection(sortDirection, details);
        var normalizedPage = page ?? DefaultPage;
        var normalizedPageSize = pageSize ?? DefaultPageSize;

        if (normalizedPage < 1)
        {
            details.Add(new ApiErrorDetail(
                "page",
                "Page must be greater than or equal to 1."));
        }

        if (normalizedPageSize is < 1 or > MaximumPageSize)
        {
            details.Add(new ApiErrorDetail(
                "pageSize",
                $"Page size must be between 1 and {MaximumPageSize}."));
        }

        var skip = ((long)normalizedPage - 1) * normalizedPageSize;
        if (skip > int.MaxValue)
        {
            details.Add(new ApiErrorDetail(
                "page",
                "The requested page is outside the supported range."));
        }

        if (details.Count > 0)
        {
            throw new StudentListValidationException(details);
        }

        return new GetStudentsQuery(
            normalizedStatus,
            normalizedSort,
            descending,
            normalizedPage,
            normalizedPageSize);
    }

    private static string? NormalizeStatus(
        string? status,
        List<ApiErrorDetail> details)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var normalized = status.Trim().ToUpperInvariant();
        if (normalized is "ACTIVE" or "INACTIVE" or "BLOCKED")
        {
            return normalized;
        }

        details.Add(new ApiErrorDetail(
            "status",
            "Status must be ACTIVE, INACTIVE, or BLOCKED."));
        return null;
    }

    private static StudentSortField NormalizeSort(
        string? sortBy,
        List<ApiErrorDetail> details)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return StudentSortField.CreatedAt;
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "createdat" => StudentSortField.CreatedAt,
            "displayname" => StudentSortField.DisplayName,
            "email" => StudentSortField.Email,
            _ => AddInvalidSort(details),
        };
    }

    private static StudentSortField AddInvalidSort(
        List<ApiErrorDetail> details)
    {
        details.Add(new ApiErrorDetail(
            "sortBy",
            "Sort field must be createdAt, displayName, or email."));
        return StudentSortField.CreatedAt;
    }

    private static bool NormalizeDirection(
        string? sortDirection,
        List<ApiErrorDetail> details)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
        {
            return true;
        }

        return sortDirection.Trim().ToLowerInvariant() switch
        {
            "asc" => false,
            "desc" => true,
            _ => AddInvalidDirection(details),
        };
    }

    private static bool AddInvalidDirection(
        List<ApiErrorDetail> details)
    {
        details.Add(new ApiErrorDetail(
            "sortDirection",
            "Sort direction must be asc or desc."));
        return true;
    }
}
