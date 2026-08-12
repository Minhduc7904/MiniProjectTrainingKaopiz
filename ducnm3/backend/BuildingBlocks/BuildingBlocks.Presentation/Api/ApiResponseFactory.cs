using BuildingBlocks.Contracts.Api;

namespace BuildingBlocks.Presentation.Api;

public static class ApiResponseFactory
{
    public static ApiResponse<TData> Success<TData>(TData data, string traceId) =>
        new(data, new ResponseMeta(traceId));

    public static ApiErrorResponse Error(
        string code,
        string message,
        string traceId,
        IReadOnlyList<ApiErrorDetail>? details = null) =>
        new(
            new ApiError(code, message, details ?? []),
            new ResponseMeta(traceId));
}
