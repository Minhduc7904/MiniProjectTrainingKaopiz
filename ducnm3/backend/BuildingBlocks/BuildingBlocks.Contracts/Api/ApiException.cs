namespace BuildingBlocks.Contracts.Api;

/// <summary>
/// Base exception mang đủ dữ liệu để presentation layer chuyển lỗi nghiệp vụ/hạ tầng thành response an toàn.
/// Kế thừa class này khi caller cần kiểm soát HTTP status và error code thay vì để middleware trả lỗi 500.
/// </summary>
public abstract class ApiException : Exception
{
    /// <summary>Khởi tạo lỗi API với code, thông điệp public, HTTP status và chi tiết field tùy chọn.</summary>
    protected ApiException(
        string errorCode,
        string safeMessage,
        int statusCode,
        IReadOnlyList<ApiErrorDetail>? details = null)
        : base(safeMessage)
    {
        ErrorCode = errorCode;
        SafeMessage = safeMessage;
        StatusCode = statusCode;
        Details = details ?? [];
    }

    public string ErrorCode { get; }

    public string SafeMessage { get; }

    public int StatusCode { get; }

    public IReadOnlyList<ApiErrorDetail> Details { get; }
}

/// <summary>Lỗi 503 cho dependency hoặc service downstream không sẵn sàng.</summary>
public sealed class ServiceUnavailableException : ApiException
{
    /// <summary>Tạo lỗi 503 từ error code và thông điệp có thể trả về client.</summary>
    public ServiceUnavailableException(
        string errorCode,
        string safeMessage)
        : base(errorCode, safeMessage, 503)
    {
    }
}
