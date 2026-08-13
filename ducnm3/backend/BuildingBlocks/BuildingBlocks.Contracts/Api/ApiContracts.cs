namespace BuildingBlocks.Contracts.Api;

public sealed record ApiResponse<TData>(TData Data, ResponseMeta Meta);

public sealed record ApiErrorResponse(ApiError Error, ResponseMeta Meta);

public sealed record ResponseMeta(string TraceId);

public sealed record ApiError(
    string Code,
    string Message,
    IReadOnlyList<ApiErrorDetail> Details);

public sealed record ApiErrorDetail(string Field, string Message);

public sealed record ServiceHealthResponse(
    string Service,
    string Status,
    DatabaseHealthResponse Database,
    MessagingHealthResponse? Messaging = null);

public sealed record DatabaseHealthResponse(string Status);

public sealed record StorageHealthResponse(string Status);

public sealed record MessagingHealthResponse(string Status);

public sealed record MediaServiceHealthResponse(
    string Service,
    string Status,
    DatabaseHealthResponse Database,
    StorageHealthResponse Storage,
    MessagingHealthResponse? Messaging = null);

public sealed record ServiceInfoResponse(string Service, string Status);
