# API Gateway Component Tests

## Scope

Project: `backend/BuildingBlocks/BuildingBlocks.Presentation.Tests`
Source: `Gateway/GatewaySwaggerDocumentTests.cs`
Dependency: ASP.NET Core `TestServer` and a fake `HttpMessageHandler`; không gọi
downstream service thật.

Chạy:

```bash
dotnet test backend/BuildingBlocks/BuildingBlocks.Presentation.Tests/BuildingBlocks.Presentation.Tests.csproj
```

## Test cases

| Test | Setup và thao tác | Pass khi |
| --- | --- | --- |
| `GatewaySwaggerDocumentReturnsServiceUnavailableWhenDownstreamRequestFails` | Đăng ký named `HttpClient` cho `course-service` với handler ném `HttpRequestException`; map Swagger proxy `/course`; gọi `GET /course/swagger/v1/swagger.json`. | Gateway trả HTTP `503` và shared error envelope có `error.code = SERVICE_UNAVAILABLE`. |

Case này bảo vệ failure mapping của Swagger document proxy. Nó không xác minh
YARP route tới Course Service đang chạy; việc đó thuộc gateway integration hoặc
cross-service test khi được bổ sung.
