# Shared Presentation Component Tests

## Scope

Project: `backend/BuildingBlocks/BuildingBlocks.Presentation.Tests`
Source: `Endpoints/DatabaseHealthEndpointTests.cs`
Dependency: ASP.NET Core `TestServer`; database probe là fake in-memory.

Chạy:

```bash
dotnet test backend/BuildingBlocks/BuildingBlocks.Presentation.Tests/BuildingBlocks.Presentation.Tests.csproj
```

## Test cases

| Case | Fake dependency | Request | Pass khi |
| --- | --- | --- | --- |
| Database healthy | `IDatabaseHealthProbe.CheckAsync` trả `IsHealthy = true`. | `GET /health` qua `TestServer`. | HTTP `200`; `data.database.status = healthy`. |
| Database unavailable | `IDatabaseHealthProbe.CheckAsync` trả `IsHealthy = false`. | `GET /health` qua `TestServer`. | HTTP `503`; `error.code = DATABASE_UNAVAILABLE`. |

Test này xác nhận `MapDatabaseHealthEndpoint` ánh xạ đúng probe result sang shared
HTTP response envelope. Nó không kiểm tra kết nối MySQL thật; kiểm tra đó thuộc
service integration test khi được bổ sung.
