# Shared Presentation Unit Tests

## Scope

Project: `backend/BuildingBlocks/BuildingBlocks.Presentation.Tests`
Source: `Middleware/ApiMiddlewareTests.cs`
Dependency: không gọi network, database, hoặc Docker.

Chạy:

```bash
dotnet test backend/BuildingBlocks/BuildingBlocks.Presentation.Tests/BuildingBlocks.Presentation.Tests.csproj
```

## Test cases

| Test | Setup và thao tác | Pass khi |
| --- | --- | --- |
| `CorrelationIdMiddlewareGeneratesAndPropagatesTraceIdentifier` | Tạo `DefaultHttpContext` không có correlation ID, gọi `CorrelationIdMiddleware.InvokeAsync`. | `TraceIdentifier` được tạo không rỗng; header `X-Correlation-Id` trên request và response đều bằng đúng `TraceIdentifier`. |
| `ExceptionMiddlewareReturnsStandardServiceUnavailableEnvelope` | Dùng `DefaultHttpContext` có trace ID `trace-id`; next middleware ném `ServiceUnavailableException`. Gọi `ApiExceptionHandlingMiddleware.InvokeAsync`. | HTTP status là `503`; response có `error.code = SERVICE_UNAVAILABLE`; `meta.traceId = trace-id`. |

Hai test bảo vệ response middleware dùng shared API contract, không phụ thuộc
triển khai của một service cụ thể.
