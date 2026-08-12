# Media Service Component Tests

## Scope

Project: `backend/Services/Media/MediaService.UnitTests`
Source: `Endpoints/MediaHealthEndpointTests.cs`
Dependency: ASP.NET Core `TestServer`; database và storage probes đều là fake
in-memory.

Chạy:

```bash
dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj
```

## Test cases

`HealthEndpointReportsEachDependencyCombination` chạy bốn data-driven cases:

| Database probe | Storage probe | Expected HTTP | Pass khi response |
| --- | --- | --- | --- |
| healthy | healthy | `200` | Có `data.status = healthy`, `data.database.status = healthy`, và `data.storage.status = healthy`. |
| unhealthy | healthy | `503` | Shared error envelope có `error.code = DATABASE_UNAVAILABLE`. |
| healthy | unhealthy | `503` | Shared error envelope có `error.code = STORAGE_UNAVAILABLE`. |
| unhealthy | unhealthy | `503` | Shared error envelope có `error.code = DEPENDENCY_UNAVAILABLE`. |

Test xác nhận endpoint chạy với hai dependency result đã biết và chọn đúng API
contract. Nó không mở MySQL/MinIO thật; vòng đời storage thật thuộc integration
test.
