// File: backend/Services/Ai/AiService.Application/UseCases/Models/GetList/GetAiModelsQuery.cs
// Mục đích: Normalize và validate filter, offset pagination trước khi query AI model.

using AiService.Application.Common;
using AiService.Application.Common.Errors;
using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Models.GetList;

public sealed record GetAiModelsQuery
{
    private GetAiModelsQuery(Guid? providerId, string? status, string? search, int page, int pageSize) =>
        (ProviderId, Status, Search, Page, PageSize) = (providerId, status, search, page, pageSize);

    public Guid? ProviderId { get; }
    public string? Status { get; }
    public string? Search { get; }
    public int Page { get; }
    public int PageSize { get; }

    public static GetAiModelsQuery Create(Guid? providerId, string? status, string? search, int? page, int? pageSize)
    {
        var details = new List<ApiErrorDetail>();
        if (providerId == Guid.Empty) details.Add(new ApiErrorDetail("providerId", "providerId must be a valid UUID."));
        string? normalizedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            normalizedStatus = status.Trim().ToUpperInvariant();
            if (!AiStatuses.IsSupported(normalizedStatus))
            {
                details.Add(new ApiErrorDetail("status", $"Status must be {AiStatuses.Active} or {AiStatuses.Inactive}."));
            }
        }

        var (normalizedPage, normalizedPageSize) = AiListPaging.Normalize(page, pageSize, details);
        if (details.Count > 0) throw AiErrors.ValidationFailed(details);
        return new GetAiModelsQuery(
            providerId,
            normalizedStatus,
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            normalizedPage,
            normalizedPageSize);
    }
}
