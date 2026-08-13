# Kiểm thử đơn vị Student Service

## Phạm vi

Dự án: `backend/Services/Student/StudentService.UnitTests`
Mã nguồn: `GetStudentByIdHandlerTests.cs` và `StudentEndpointTests.cs`
Thành phần phụ thuộc: không có MySQL hoặc external network; endpoint test dùng
ASP.NET Core `TestServer`.

Chạy:

```bash
dotnet test backend/Services/Student/StudentService.UnitTests/StudentService.UnitTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `ExistingStudentIsReturned` | Repository stub trả một `StudentDetails` tồn tại. | Handler trả nguyên thông tin `id`, `email`, `displayName`, `status` để Media Service có thể parse response envelope. |
| `MissingStudentReturnsNotFound` | Repository stub trả `null`. | Handler ném `StudentApplicationException` có code `STUDENT_NOT_FOUND`. |
| `GetStudentMapsExpectedStatus` | Gửi lần lượt ID tồn tại, UUID không tồn tại và chuỗi không phải UUID qua `TestServer`. | Endpoint lần lượt trả `200`, `404`, `400` theo response envelope dùng chung. |

Tests hiện bao phủ lookup Học viên thành công/không tồn tại, validation UUID và
status mapping ở HTTP endpoint. Typed-client Media → Student và lookup với MySQL
thật chưa có integration test chuyên biệt.
