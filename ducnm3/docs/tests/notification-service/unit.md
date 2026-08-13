# Kiểm thử đơn vị Notification Service

## Phạm vi

Dự án: `backend/Services/Notification/NotificationService.UnitTests`
Mã nguồn: `UnitTest1.cs`
Thành phần phụ thuộc: không có MySQL hoặc mạng.

Chạy:

```bash
dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `NotificationDatabaseHealthProbe` với chuỗi kết nối có cổng không hợp lệ; hủy token trước khi gọi `CheckAsync`. | Trình kiểm tra ném `OperationCanceledException`; thao tác hủy yêu cầu không bị bỏ qua hoặc ánh xạ thành `IsHealthy = false`. |

Kiểm thử hiện tại chỉ bảo vệ hành vi hủy của trình kiểm tra sức khỏe cơ sở dữ liệu. Chưa
có kiểm thử tích hợp MySQL hoặc quy trình thông báo trong dự án này.
