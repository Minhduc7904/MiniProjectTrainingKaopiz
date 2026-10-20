// File: backend/Services/Ai/AiService.Application/UseCases/Providers/GetList/GetAiProvidersQuery.cs
// Mục đích: Normalize và validate filter, offset pagination trước khi query AI provider.

using AiService.Application.Common;
using AiService.Application.Common.Errors;
using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Providers.GetList;

public sealed record GetAiProvidersQuery
{
    private GetAiProvidersQuery(string? status, string? providerType, string? search, int page, int pageSize) =>
        (Status, ProviderType, Search, Page, PageSize) = (status, providerType, search, page, pageSize);

    public string? Status { get; }
    public string? ProviderType { get; }
    public string? Search { get; }
    public int Page { get; }
    public int PageSize { get; }

    public static GetAiProvidersQuery Create(string? status, string? providerType, string? search, int? page, int? pageSize)
    {
        var details = new List<ApiErrorDetail>();
        string? normalizedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            normalizedStatus = status.Trim().ToUpperInvariant();
            if (!AiStatuses.IsSupported(normalizedStatus))
            {
                details.Add(new ApiErrorDetail("status", $"Status must be {AiStatuses.Active} or {AiStatuses.Inactive}."));
            }
        }

        string? normalizedType = null;
        if (!string.IsNullOrWhiteSpace(providerType))
        {
            normalizedType = providerType.Trim().ToUpperInvariant();
            if (!AiProviderTypes.IsSupported(normalizedType))
            {
                details.Add(new ApiErrorDetail("providerType", $"Provider type must be one of {string.Join(", ", AiProviderTypes.All)}."));
            }
        }

        var (normalizedPage, normalizedPageSize) = AiListPaging.Normalize(page, pageSize, details);
        if (details.Count > 0) throw AiErrors.ValidationFailed(details);
        return new GetAiProvidersQuery(
            normalizedStatus,
            normalizedType,
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            normalizedPage,
            normalizedPageSize);
    }
}
