# Notification Service Unit Tests

## Scope

Project: `backend/Services/Notification/NotificationService.UnitTests`
Source: `UnitTest1.cs`
Dependency: không có MySQL hoặc network.

Chạy:

```bash
dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj
```

## Test cases

| Test | Setup và thao tác | Pass khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `NotificationDatabaseHealthProbe` với connection string cổng không hợp lệ; huỷ token trước khi gọi `CheckAsync`. | Probe ném `OperationCanceledException`; request cancellation không bị nuốt hoặc ánh xạ thành `IsHealthy = false`. |

Test hiện tại chỉ bảo vệ cancellation behavior của database health probe. Chưa có
test integration MySQL hoặc workflow notification trong project này.
