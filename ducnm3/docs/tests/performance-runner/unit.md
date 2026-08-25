# Unit test Performance Runner

## Phạm vi

Dự án: `backend/Tools/Lms.PerformanceRunner.UnitTests/Lms.PerformanceRunner.UnitTests.csproj`.

`Http/NotificationBenchmarkClientTests.cs` dùng `HttpMessageHandler` trong
memory, không gọi Gateway, Docker, RabbitMQ hoặc database.

## Ca kiểm thử

- `RunAsyncWhenActorIdConfiguredInEnvironmentSendsAdminActorHeadersOnEveryRequest`:
  khi `PERFORMANCE_ACTOR_ID` có UUID hợp lệ, `NotificationBenchmarkClient` phải
  gửi `X-Actor-Type: ADMIN` và `X-Actor-Id` cho request tạo batch cũng như các
  request poll trạng thái; response giả hoàn tất với `COMPLETED`.

Chạy:

```bash
dotnet test backend/Tools/Lms.PerformanceRunner.UnitTests/Lms.PerformanceRunner.UnitTests.csproj
```
