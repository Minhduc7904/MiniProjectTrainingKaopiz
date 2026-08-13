# Kiểm thử thành phần lớp Presentation dùng chung

## Phạm vi

Dự án: `backend/BuildingBlocks/BuildingBlocks.Presentation.Tests`
Mã nguồn: `Endpoints/DatabaseHealthEndpointTests.cs`
Thành phần phụ thuộc: ASP.NET Core `TestServer`; trình kiểm tra cơ sở dữ liệu là đối tượng
giả trong bộ nhớ.

Chạy:

```bash
dotnet test backend/BuildingBlocks/BuildingBlocks.Presentation.Tests/BuildingBlocks.Presentation.Tests.csproj
```

## Ca kiểm thử

| Trường hợp | Thành phần phụ thuộc giả | Yêu cầu | Đạt khi |
| --- | --- | --- | --- |
| Cơ sở dữ liệu khỏe mạnh | `IDatabaseHealthProbe.CheckAsync` trả `IsHealthy = true`. | `GET /health` qua `TestServer`. | HTTP `200`; `data.database.status = healthy`. |
| Cơ sở dữ liệu không khả dụng | `IDatabaseHealthProbe.CheckAsync` trả `IsHealthy = false`. | `GET /health` qua `TestServer`. | HTTP `503`; `error.code = DATABASE_UNAVAILABLE`. |

Kiểm thử này xác nhận `MapDatabaseHealthEndpoint` ánh xạ đúng kết quả kiểm tra sang
vỏ phản hồi HTTP dùng chung. Nó không kiểm tra kết nối MySQL thật; kiểm tra đó
thuộc kiểm thử tích hợp dịch vụ khi được bổ sung.
