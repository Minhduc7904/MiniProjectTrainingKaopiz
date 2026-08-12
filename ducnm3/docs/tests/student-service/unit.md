# Student Service Unit Tests

## Scope

Project: `backend/Services/Student/StudentService.UnitTests`
Source: `UnitTest1.cs`
Dependency: không có MySQL hoặc network.

Chạy:

```bash
dotnet test backend/Services/Student/StudentService.UnitTests/StudentService.UnitTests.csproj
```

## Test cases

| Test | Setup và thao tác | Pass khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `StudentDatabaseHealthProbe` với connection string cổng không hợp lệ; huỷ token trước khi gọi `CheckAsync`. | `OperationCanceledException` được propagate; cancellation không bị trả thành kết quả database lỗi. |

Test hiện tại giới hạn ở cancellation contract của health probe. Test integration
với MySQL thật chưa được tạo.
