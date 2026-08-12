namespace BuildingBlocks.Contracts.Api;

public abstract class ApiException : Exception
{
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

public sealed class ServiceUnavailableException : ApiException
{
    public ServiceUnavailableException(
        string errorCode,
        string safeMessage)
        : base(errorCode, safeMessage, 503)
    {
    }
}
