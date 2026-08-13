# Hướng dẫn kiểm thử

## Nền tảng kiểm thử và cách tổ chức

Tất cả kiểm thử backend sử dụng NUnit. Các project kiểm thử được đặt tên theo
component sở hữu và nằm cạnh component đó:

```text
backend/
├── BuildingBlocks/BuildingBlocks.Presentation.Tests/
│   ├── Middleware/
│   ├── Endpoints/
│   └── Gateway/
├── Services/<Service>/
    ├── <Service>Service.UnitTests/
    └── <Service>Service.IntegrationTests/
└── Tools/
    ├── Lms.DataSeeder.UnitTests/
    └── Lms.DataSeeder.IntegrationTests/
```

Không tạo project kiểm thử rỗng. Chỉ thêm project kiểm thử cho service khi có
hành vi đầu tiên thuộc trách nhiệm của service đó.

## Danh mục kiểm thử chi tiết

Danh mục kiểm thử có thể thực thi được tổ chức trong
[`../tests/README.md`](../tests/README.md), sau đó phân theo service hoặc
component dùng chung sở hữu và loại kiểm thử. Mỗi trang ghi lại tên kiểm thử
nguồn, bước chuẩn bị, trạng thái đầu vào/dependency, kết quả mong đợi và điều
kiện đạt chính xác.

## Chạy kiểm thử

Chạy toàn bộ kiểm thử backend:

```bash
dotnet test backend/Lms.sln -m:1
```

Chạy kiểm thử cho một service:

```bash
dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj
dotnet test backend/Services/Student/StudentService.UnitTests/StudentService.UnitTests.csproj
dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj
dotnet test backend/Services/Media/MediaService.IntegrationTests/MediaService.IntegrationTests.csproj
dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj
dotnet test backend/Services/Scheduler/SchedulerService.UnitTests/SchedulerService.UnitTests.csproj
dotnet test backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj
dotnet test backend/Tools/Lms.DataSeeder.IntegrationTests/Lms.DataSeeder.IntegrationTests.csproj
```

## Các loại kiểm thử

| Loại | Phạm vi | Phụ thuộc bên ngoài |
| --- | --- | --- |
| Đơn vị | Domain, Application, hành vi Infrastructure có tính xác định | Không có; dùng interface giả lập |
| Component | Middleware, endpoint API tối giản, cấu trúc bao phản hồi | `TestServer`, không dùng mạng hoặc database |
| Tích hợp service | Adapter lưu trữ, API, SQL migration, repository, DbContext đã scaffold | Các container phụ thuộc cô lập |
| Tích hợp Gateway | Tiền tố YARP, lỗi từ service phía sau, proxy Swagger | TestServer và HTTP handler phía sau giả |
| Tích hợp liên service | Ranh giới service thông qua Gateway | Docker Compose hoặc Testcontainer chuyên biệt |
| Đầu cuối | Luồng trình duyệt xuyên suốt frontend và backend | Playwright cùng ngăn xếp hệ thống cô lập |

## Thêm kiểm thử mới

1. Đặt kiểm thử quy tắc nghiệp vụ trong `<Service>Service.UnitTests`.
2. Thêm kiểm thử endpoint hoặc middleware vào project kiểm thử của component sở hữu.
3. Chỉ thêm project kiểm thử tích hợp service khi thay đổi cần MySQL thật, SQL migration hoặc hành vi của DbContext được sinh.
4. Đặt kiểm thử liên service bên ngoài từng service riêng lẻ, trong thư mục gốc `tests/`.
5. Mỗi kiểm thử phải tự tạo dữ liệu và dọn dẹp thông qua môi trường kiểm thử cô lập của chính nó.

## Kiểm thử storage của Media Service

`MediaService.UnitTests` bao phủ việc kiểm tra hợp lệ option storage, ánh xạ
bucket, sinh object key theo UTC, kiểm tra hợp lệ request upload, hủy thao tác
database và cả bốn tổ hợp trạng thái database/MinIO.

`MediaService.IntegrationTests` khởi động một MinIO Testcontainer cô lập. Dự án
này tạo đủ năm bucket và kiểm tra upload, tồn tại, download, metadata, xóa, ánh
xạ category và trạng thái storage mà không phụ thuộc vào Docker Compose stack
của lập trình viên. Docker phải đang chạy để thực thi dự án này.

## Kiểm thử Scheduler Service

`SchedulerService.UnitTests` hiện kiểm tra
`SchedulerDatabaseHealthProbe` truyền tiếp yêu cầu hủy. Việc ánh xạ phản hồi
trạng thái sẵn sàng của database được bao phủ bởi kiểm thử component presentation
dùng chung vì Scheduler sử dụng ánh xạ `MapDatabaseHealthEndpoint` chung.
Chưa có kiểm thử thực thi job: trong giai đoạn này, Worker chủ đích chưa có vòng
lặp polling, claiming, phân tích CRON hoặc xử lý handler.

## Kiểm thử Data Seeder

`Lms.DataSeeder.UnitTests` bao phủ việc sinh UUID/dữ liệu có tính xác định, phạm
vi quan hệ, tính toán kế hoạch chính xác và các chốt an toàn cho môi trường
Development.

`Lms.DataSeeder.IntegrationTests` khởi động các container MySQL 8.4 cô lập cho
Student và Course, áp dụng migration V001 thật, đồng thời kiểm tra số lượng dữ
liệu seed, tính duy nhất của quan hệ, tính idempotent khi resume và việc từ chối
database không rỗng. Kiểm thử này không bao giờ sử dụng các database Compose
trên máy cục bộ.
