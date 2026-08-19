# Kiểm thử đơn vị Course Service

## Phạm vi

Dự án: `backend/Services/Course/CourseService.UnitTests`
Mã nguồn: `Features/Courses/GetList/GetCoursesHandlerTests.cs` và health probe.
Thành phần phụ thuộc: không có MySQL hoặc mạng.

Chạy:

```bash
dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `CourseDatabaseHealthProbe` với chuỗi kết nối trỏ tới cổng không hợp lệ; hủy `CancellationToken` trước khi gọi `CheckAsync`. | `CheckAsync` ném `OperationCanceledException`, không chuyển thao tác hủy thành trạng thái cơ sở dữ liệu không khỏe mạnh. |
| `HandleAsync_ValidOffsetQuery_UsesPagedRepositoryOnly` | Tạo query `status=published` với repository double. | Query được normalize thành `PUBLISHED`; use case chỉ gọi `GetPagedAsync`, không gọi `GetAllAsync`. |
| Export CSV | Test `CsvRowWriter`, export query/chunk và handler với repository double. | BOM/header đúng, escaping RFC 4180, status được validate và export chỉ gọi chunk reader với cancellation token. |

Ca này bảo đảm thao tác tắt/hủy yêu cầu được tôn trọng. Nó không kiểm tra tính
khả dụng của MySQL thật. Ca list bảo vệ việc endpoint phân trang không vô tình
materialize toàn bộ Course.
