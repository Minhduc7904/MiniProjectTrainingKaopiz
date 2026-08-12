# Course Service Unit Tests

## Scope

Project: `backend/Services/Course/CourseService.UnitTests`
Source: `UnitTest1.cs`
Dependency: không có MySQL hoặc network.

Chạy:

```bash
dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj
```

## Test cases

| Test | Setup và thao tác | Pass khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `CourseDatabaseHealthProbe` với connection string trỏ tới cổng không hợp lệ; huỷ `CancellationToken` trước khi gọi `CheckAsync`. | `CheckAsync` ném `OperationCanceledException`, không chuyển cancellation thành trạng thái database unhealthy. |

Case này đảm bảo shutdown/request-abort được tôn trọng. Nó không kiểm tra
availability của MySQL thật.
