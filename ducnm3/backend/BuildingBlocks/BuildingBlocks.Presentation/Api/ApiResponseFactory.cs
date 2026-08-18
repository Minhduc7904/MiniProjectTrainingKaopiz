using BuildingBlocks.Contracts.Api;

namespace BuildingBlocks.Presentation.Api;

/// <summary>Factory tạo response envelope thống nhất; endpoint và middleware dùng thay vì tự dựng JSON shape.</summary>
public static class ApiResponseFactory
{
    /// <summary>Tạo response thành công không phân trang từ dữ liệu và trace ID hiện hành.</summary>
    public static ApiResponse<TData> Success<TData>(TData data, string traceId) =>
        new(data, new ResponseMeta(traceId));

    /// <summary>Tạo response thành công có metadata phân trang.</summary>
    public static ApiResponse<TData> Success<TData>(
        TData data,
        string traceId,
        PaginationMeta pagination) =>
        new(data, new ResponseMeta(traceId, pagination));

    /// <summary>Tạo error envelope, thay null details bằng danh sách rỗng để contract JSON ổn định.</summary>
    public static ApiErrorResponse Error(
        string code,
        string message,
        string traceId,
        IReadOnlyList<ApiErrorDetail>? details = null) =>
        new(
            new ApiError(code, message, details ?? []),
            new ResponseMeta(traceId));
}
