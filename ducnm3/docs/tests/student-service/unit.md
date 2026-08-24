# Kiểm thử đơn vị Student Service

## Phạm vi

Dự án: `backend/Services/Student/StudentService.UnitTests`
Mã nguồn:

- `GetStudentByIdHandlerTests.cs`.
- `GetStudentsQueryTests.cs`.
- `GetStudentsHandlerTests.cs`.
- `ApiRoutesTests.cs`.

Các test mới chỉ kiểm tra Application/shared route trong process, không dùng
MySQL, Docker hoặc external network. `StudentEndpointTests.cs` là component test
legacy và sẽ được chuyển khỏi project unit khi phần GET detail được chạm tới.

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
| `DefaultValuesReturnOffsetDefaults` | Tạo list query không truyền parameter. | Query dùng `createdAt desc`, page `1`, pageSize `20`, không filter status/search. |
| `ValidValuesNormalizeAllowlistedQuery` | Truyền search, status/direction khác hoa thường, `displayName`, page `2`, pageSize `100`. | Query trim search và chuẩn hóa đúng allowlist/boundaries. |
| `InvalidValuesReturnAllValidationDetails` | Truyền đồng thời status/sort/direction/page/pageSize sai. | Exception có `VALIDATION_FAILED` và đủ năm field details. |
| `PageOffsetExceedsProviderLimitReturnsValidationError` | Truyền page tạo offset vượt giới hạn `Skip(int)`. | Query bị từ chối trước khi gọi repository. |
| `ValidQueryReturnsRepositoryPage` | Handler nhận query hợp lệ và repository spy. | Trả đúng page và truyền nguyên query/cancellation token đúng một lần. |

Tests hiện bao phủ lookup detail và validation/orchestration cho GET list.
HTTP list contract và MySQL pagination được ghi riêng tại `component.md` và
`integration.md`.
