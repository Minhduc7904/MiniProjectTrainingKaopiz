# Kiểm thử đơn vị Course Service

## Phạm vi

Dự án: `backend/Services/Course/CourseService.UnitTests`
Mã nguồn: `UnitTest1.cs`
Thành phần phụ thuộc: không có MySQL hoặc mạng.

Chạy:

```bash
dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `CourseDatabaseHealthProbe` với chuỗi kết nối trỏ tới cổng không hợp lệ; hủy `CancellationToken` trước khi gọi `CheckAsync`. | `CheckAsync` ném `OperationCanceledException`, không chuyển thao tác hủy thành trạng thái cơ sở dữ liệu không khỏe mạnh. |

Ca này bảo đảm thao tác tắt/hủy yêu cầu được tôn trọng. Nó không kiểm tra tính
khả dụng của MySQL thật.
