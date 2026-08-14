# Kiểm thử đơn vị Notification Service

## Phạm vi

Dự án: `backend/Services/Notification/NotificationService.UnitTests`
Mã nguồn: `UnitTest1.cs`, `BatchNotificationTests.cs`, `NotificationBatchEndpointTests.cs`
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
có kiểm thử tích hợp MySQL hoặc RabbitMQ trong dự án này.

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `HandleAsync_RejectsScopeOtherThanAllStudents` | Gọi Create handler với `COURSE_ENROLLED`. | Trả validation 400 trước khi gọi Student Service. |
| `HandleAsync_CreatesSnapshotAndDispatchesCommand` | Student client giả trả hai UUID. | Tạo snapshot và phát đúng `DispatchNotificationBatchV1`. |
| `FakeNotificationSenderTests` | Sinh UUID có hash thỏa từng rule. | Lần một/lần hai thất bại đúng điều kiện `% 20`/`% 100`. |
| `HandleAsync_FirstBusinessFailureMarksItemForRetryAndRequeues` | Sender giả ném lỗi ở lần gửi đầu. | Item được đánh dấu thất bại nghiệp vụ và command được phát lại. |
| `NotificationBatchEndpointTests` | TestServer map POST/GET cùng doubles in-memory. | POST trả 202 + Location, GET trả 200, scope lạ trả 400. |
