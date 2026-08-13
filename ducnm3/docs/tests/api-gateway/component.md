# Kiểm thử thành phần API Gateway

## Phạm vi

Dự án: `backend/BuildingBlocks/BuildingBlocks.Presentation.Tests`
Mã nguồn: `Gateway/GatewaySwaggerDocumentTests.cs`
Thành phần phụ thuộc: ASP.NET Core `TestServer` và `HttpMessageHandler` giả; không
gọi dịch vụ hạ nguồn thật.

Chạy:

```bash
dotnet test backend/BuildingBlocks/BuildingBlocks.Presentation.Tests/BuildingBlocks.Presentation.Tests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `GatewaySwaggerDocumentReturnsServiceUnavailableWhenDownstreamRequestFails` | Đăng ký `HttpClient` có tên cho `course-service` với bộ xử lý ném `HttpRequestException`; ánh xạ proxy Swagger `/course`; gọi `GET /course/swagger/v1/swagger.json`. | Gateway trả HTTP `503` và vỏ lỗi dùng chung có `error.code = SERVICE_UNAVAILABLE`. |

Ca này bảo vệ ánh xạ lỗi của proxy tài liệu Swagger. Nó không xác minh tuyến YARP
tới Course Service đang chạy; việc đó thuộc kiểm thử tích hợp gateway hoặc kiểm
thử liên dịch vụ khi được bổ sung.
