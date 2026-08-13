using BuildingBlocks.Contracts.Api;

namespace BuildingBlocks.Presentation.Api;

public static class ApiResponseFactory
{
    public static ApiResponse<TData> Success<TData>(TData data, string traceId) =>
        new(data, new ResponseMeta(traceId));

    public static ApiResponse<TData> Success<TData>(
        TData data,
        string traceId,
        PaginationMeta pagination) =>
        new(data, new ResponseMeta(traceId, pagination));

    public static ApiErrorResponse Error(
        string code,
        string message,
        string traceId,
        IReadOnlyList<ApiErrorDetail>? details = null) =>
        new(
            new ApiError(code, message, details ?? []),
            new ResponseMeta(traceId));
}
