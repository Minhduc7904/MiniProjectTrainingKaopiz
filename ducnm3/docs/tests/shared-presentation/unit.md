# Kiểm thử đơn vị lớp Presentation dùng chung

## Phạm vi

Dự án: `backend/BuildingBlocks/BuildingBlocks.Presentation.Tests`
Mã nguồn: `Middleware/ApiMiddlewareTests.cs`
Thành phần phụ thuộc: không gọi mạng, cơ sở dữ liệu hoặc Docker.

Chạy:

```bash
dotnet test backend/BuildingBlocks/BuildingBlocks.Presentation.Tests/BuildingBlocks.Presentation.Tests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `CorrelationIdMiddlewareGeneratesAndPropagatesTraceIdentifier` | Tạo `DefaultHttpContext` không có ID tương quan, gọi `CorrelationIdMiddleware.InvokeAsync`. | `TraceIdentifier` được tạo không rỗng; tiêu đề `X-Correlation-Id` trên yêu cầu và phản hồi đều bằng đúng `TraceIdentifier`. |
| `ExceptionMiddlewareReturnsStandardServiceUnavailableEnvelope` | Dùng `DefaultHttpContext` có ID theo dõi `trace-id`; middleware tiếp theo ném `ServiceUnavailableException`. Gọi `ApiExceptionHandlingMiddleware.InvokeAsync`. | Trạng thái HTTP là `503`; phản hồi có `error.code = SERVICE_UNAVAILABLE`; `meta.traceId = trace-id`. |

Hai kiểm thử bảo vệ middleware phản hồi dùng hợp đồng API chung, không phụ thuộc
triển khai của một dịch vụ cụ thể.
