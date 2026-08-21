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
| `HandleAsyncCreatesDraftAndSynchronizesMediaEmbeddedInDescription` | Gọi `CreateCourseHandler` với tên có khoảng trắng và Markdown image Media public. | Handler trim tên, luôn gọi repository với `DRAFT` và gửi đúng command `COURSE_DESCRIPTION` đến Media Service. |
| `HandleAsync_ExistingCourse_DeletesAggregateAndQueuesEveryUsageId` | Repository double trả một Lesson và Media reader trả usage của Course/Lesson. | Handler snapshot đủ năm owner scope, hard-delete Course rồi gửi một `DeleteMediaUsagesByIdsV1` chứa toàn bộ usage ID. |
| `Render_RelativeMediaContentPath_PrefixesConfiguredGatewayPublicBaseUrl` | Renderer nhận Markdown image với URL Media tương đối và biến môi trường Gateway cố định. | HTML trả về có `img src` tuyệt đối, bắt đầu bằng `Gateway__PublicBaseUrl`. |
| Course details (cần bổ sung) | Repository có path batch và path N+1 riêng. | Handler chỉ gọi path batch không N+1; component test xác nhận `400` với UUID sai và `404` khi không có Course. |

Ca này bảo đảm thao tác tắt/hủy yêu cầu được tôn trọng. Nó không kiểm tra tính
khả dụng của MySQL thật. Ca list bảo vệ việc endpoint phân trang không vô tình
materialize toàn bộ Course.
