# Kiểm thử đơn vị Scheduler Service

## Dự án

`backend/Services/Scheduler/SchedulerService.UnitTests/SchedulerService.UnitTests.csproj`

## `CheckAsyncPropagatesRequestCancellation`

- Mã nguồn: `Health/SchedulerDatabaseHealthProbeTests.cs`.
- Đơn vị được kiểm thử: `SchedulerDatabaseHealthProbe.CheckAsync`.
- Thiết lập: chuỗi kết nối MySQL cục bộ không thể truy cập và `NullLogger`; token hủy yêu cầu được hủy trước khi gọi.
- Trạng thái đầu vào/thành phần phụ thuộc: token đã hủy, không truy cập cơ sở dữ liệu phát triển dùng chung.
- Kết quả mong đợi: `OperationCanceledException` được truyền tiếp thay vì chuyển thành kết quả kiểm tra không khỏe mạnh.
- Điều kiện đạt: NUnit `Assert.ThrowsAsync<OperationCanceledException>` thành công.

Ánh xạ phản hồi khi cơ sở dữ liệu không khả dụng là hành vi dùng chung của
`MapDatabaseHealthEndpoint` được ghi tại
`docs/tests/shared-presentation/component.md`. Chưa có kiểm thử thực thi Scheduler
vì cơ chế thăm dò của tiến trình xử lý nền, phân tích CRON, nhận quyền chạy và gọi dịch vụ chưa
được triển khai.

Chạy:

```bash
dotnet test backend/Services/Scheduler/SchedulerService.UnitTests/SchedulerService.UnitTests.csproj
```
