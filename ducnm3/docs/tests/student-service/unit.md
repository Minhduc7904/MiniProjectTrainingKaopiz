# Kiểm thử đơn vị Student Service

## Phạm vi

Dự án: `backend/Services/Student/StudentService.UnitTests`
Mã nguồn: `UnitTest1.cs`
Thành phần phụ thuộc: không có MySQL hoặc mạng.

Chạy:

```bash
dotnet test backend/Services/Student/StudentService.UnitTests/StudentService.UnitTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `StudentDatabaseHealthProbe` với chuỗi kết nối có cổng không hợp lệ; hủy token trước khi gọi `CheckAsync`. | `OperationCanceledException` được truyền tiếp; thao tác hủy không bị trả thành kết quả cơ sở dữ liệu lỗi. |

Kiểm thử hiện tại giới hạn ở hợp đồng hủy của trình kiểm tra sức khỏe. Kiểm thử tích hợp
với MySQL thật chưa được tạo.
