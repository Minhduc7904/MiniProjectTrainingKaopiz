// File: backend/Services/Ai/AiService.Application/Common/AiListPaging.cs
// Mục đích: Normalize và validate offset pagination dùng chung cho các query list.

using BuildingBlocks.Contracts.Api;

namespace AiService.Application.Common;

public static class AiListPaging
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize, List<ApiErrorDetail> details)
    {
        var normalizedPage = page ?? DefaultPage;
        var normalizedPageSize = pageSize ?? DefaultPageSize;
        if (normalizedPage < 1) details.Add(new ApiErrorDetail("page", "Page must be greater than or equal to 1."));
        if (normalizedPageSize is < 1 or > MaximumPageSize) details.Add(new ApiErrorDetail("pageSize", $"Page size must be between 1 and {MaximumPageSize}."));
        if (((long)normalizedPage - 1) * normalizedPageSize > int.MaxValue) details.Add(new ApiErrorDetail("page", "The requested page is outside the supported range."));
        return (normalizedPage, normalizedPageSize);
    }

    public static int TotalPages(long totalItems, int pageSize) =>
        totalItems == 0 ? 0 : checked((int)Math.Ceiling(totalItems / (double)pageSize));
}
