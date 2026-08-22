# Kiểm thử thành phần lớp Presentation dùng chung

## Phạm vi

Dự án: `backend/BuildingBlocks/BuildingBlocks.Presentation.Tests`
Mã nguồn: `Endpoints/DatabaseHealthEndpointTests.cs`,
`Gateway/GatewayCorsTests.cs`
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
| Preflight CORS origin được phép | Policy `frontend` bind từ biến môi trường `Cors__AllowedOrigins__0=http://localhost:5173`. | `OPTIONS /student/api/students` kèm `Origin` và `Access-Control-Request-Method: GET`. | HTTP `204`; có `Access-Control-Allow-Origin` đúng origin; allow headers gồm `X-Correlation-Id`. |
| Preflight CORS Vercel được phép | Policy `frontend` bind từ biến môi trường `Cors__AllowedOrigins__1=https://mini-project-training-kaopiz.vercel.app`. | `OPTIONS /student/api/students` kèm origin Vercel và `Access-Control-Request-Method: GET`. | HTTP `204`; `Access-Control-Allow-Origin` echo đúng origin Vercel. |
| GET kèm origin được phép | Cùng policy. | `GET /student/api/students` với `Origin: http://localhost:5173`. | HTTP `200`; có `Access-Control-Allow-Origin`. |
| GET kèm origin lạ | Cùng policy. | `GET` với `Origin: http://evil.example`. | HTTP `200`; không echo origin lạ. |

Health endpoint xác nhận `MapDatabaseHealthEndpoint` ánh xạ đúng kết quả kiểm tra sang
vỏ phản hồi HTTP dùng chung. Nó không kiểm tra kết nối MySQL thật; kiểm tra đó
thuộc kiểm thử tích hợp dịch vụ khi được bổ sung.

`ActorPolicyEndpointTests` kiểm tra `RequireActor`: chuẩn hóa `ADMIN`, cho phép
`Any`, từ chối actor sai policy bằng `403 FORBIDDEN`, và trả `400 VALIDATION_FAILED`
khi header type/UUID không hợp lệ.
