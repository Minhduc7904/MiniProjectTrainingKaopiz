# 9. Kiến trúc sạch trong mỗi vi dịch vụ

Mỗi dịch vụ có 4 tầng:

```text
Domain
Application
Infrastructure
Api
```

Quan hệ phụ thuộc:

```text
Api
 │
 ├──────────────► Application
 │
 └──────────────► Infrastructure

Infrastructure
 │
 └──────────────► Application

Application
 │
 └──────────────► Domain

Domain
 └── không phụ thuộc tầng nào
```

---

# Cơ chế kiểm tra trạng thái và xử lý lỗi dùng chung cho API

`BuildingBlocks.Contracts` quản lý các DTO phản hồi không phụ thuộc khung phần mềm, giao diện thăm dò trạng thái, mã lỗi, tên tiêu đề HTTP và hằng số trạng thái. `BuildingBlocks.Presentation` quản lý phần mềm trung gian ASP.NET Core, ánh xạ phản hồi và các ánh xạ điểm cuối có thể tái sử dụng.

Tầng Infrastructure của mỗi dịch vụ triển khai `IDatabaseHealthProbe` bằng chuỗi kết nối MySQL riêng và truy vấn `SELECT 1`. API ánh xạ `GET /health`; nếu API vẫn truy cập được nhưng cơ sở dữ liệu không khả dụng thì trả về `503 DATABASE_UNAVAILABLE`. API Gateway ánh xạ các dịch vụ hạ nguồn không thể kết nối thành `503 SERVICE_UNAVAILABLE`.

Domain và Application không phụ thuộc vào ASP.NET Core, MySQL hoặc dự án trình bày.

Scheduler tuân theo bốn tầng tương tự và bổ sung `SchedulerService.Worker` làm
tiến trình nền độc lập trong tương lai. Worker hiện tại được chủ đích chỉ xây dựng
ở dạng khung: chưa thăm dò, nhận xử lý, phân tích CRON, thực thi bộ xử lý hay gọi
Media Service hoặc Notification Service. Siêu dữ liệu lập lịch dùng chung nằm trong
`lms_scheduler_db`; trạng thái người nhận và nội dung thông báo nằm trong
`lms_notification_db`.

## Tổ chức kiểm thử

- Hành vi API dùng chung nằm trong `BuildingBlocks.Presentation.Tests`, được phân chia theo `Middleware/`, `Endpoints/` và `Gateway/`.
- Mỗi dịch vụ sở hữu dự án `*Service.UnitTests` đặt cạnh các dự án API, Application, Domain và Infrastructure tương ứng.
- Các kiểm thử tích hợp trong tương lai dùng dự án `*Service.IntegrationTests` đặt cạnh dịch vụ tương ứng và một container MySQL thật, biệt lập.
- Kiểm thử liên dịch vụ và kiểm thử đầu cuối nằm trong thư mục gốc `tests/`, không thuộc riêng dịch vụ nào.

## Ranh giới công cụ dữ liệu phát triển

`backend/Tools/Lms.DataSeeder` là công cụ dòng lệnh phát triển chỉ chạy khi được
chủ động kích hoạt, không phải vi dịch vụ thứ sáu và không thuộc đồ thị phụ thuộc
của các dịch vụ nghiệp vụ. Công cụ ghi các tập dữ liệu lớn, xác định được trước
thông qua chuỗi kết nối Student Service/Course Service riêng để chuẩn bị kiểm thử hiệu năng cục bộ
và trình diễn. Các dự án API, Domain và Application không tham chiếu đến công cụ này.

Các dự án kiểm thử đơn vị và kiểm thử tích hợp được đặt cạnh công cụ. Kiểm thử
tích hợp áp dụng các bản thay đổi SQL thực tế do từng dịch vụ sở hữu lên các container
MySQL biệt lập; các dịch vụ ở môi trường sản xuất/thời gian chạy vẫn sở hữu
lược đồ và hành vi nghiệp vụ.

# 10. Cấu trúc thư mục tổng thể

```text
lms-mini/
│
├── backend/
│   │
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Contracts/
│   │   ├── BuildingBlocks.Shared/
│   │   ├── BuildingBlocks.DatabaseMigration/
│   │   ├── BuildingBlocks.Presentation/
│   │   └── BuildingBlocks.Presentation.Tests/
│   │
│   ├── Gateway/
│   │   └── Lms.ApiGateway/
│   │
│   ├── Services/
│   │   │
│   │   ├── Course/
│   │   │   ├── CourseService.Api/
│   │   │   ├── CourseService.Application/
│   │   │   ├── CourseService.Domain/
│   │   │   ├── CourseService.Infrastructure/
│   │   │   └── CourseService.UnitTests/
│   │   │
│   │   ├── Student/
│   │   │   ├── StudentService.Api/
│   │   │   ├── StudentService.Application/
│   │   │   ├── StudentService.Domain/
│   │   │   ├── StudentService.Infrastructure/
│   │   │   └── StudentService.UnitTests/
│   │   │
│   │   ├── Media/
│   │   │   ├── MediaService.Api/
│   │   │   ├── MediaService.Application/
│   │   │   ├── MediaService.Domain/
│   │   │   ├── MediaService.Infrastructure/
│   │   │   └── MediaService.UnitTests/
│   │   │
│   │   ├── Notification/
│   │   │   ├── NotificationService.Api/
│   │   │   ├── NotificationService.Application/
│   │   │   ├── NotificationService.Domain/
│   │   │   ├── NotificationService.Infrastructure/
│   │   │   └── NotificationService.UnitTests/
│   │   │
│   │   └── Scheduler/
│   │       ├── SchedulerService.Api/
│   │       ├── SchedulerService.Application/
│   │       ├── SchedulerService.Domain/
│   │       ├── SchedulerService.Infrastructure/
│   │       ├── SchedulerService.Worker/
│   │       └── SchedulerService.UnitTests/
│   │
│   └── Tools/
│       ├── Lms.DataSeeder/
│       ├── Lms.DataSeeder.UnitTests/
│       └── Lms.DataSeeder.IntegrationTests/
│
├── frontend/
│   └── lms-web/
│       ├── src/
│       ├── tests/
│       └── README.md
│
├── tests/
│   ├── CourseService.UnitTests/
│   ├── MediaService.UnitTests/
│   ├── NotificationService.UnitTests/
│   └── IntegrationTests/
│
├── deploy/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   │
│   ├── mysql/
│   │   └── init/
│   │
│   └── minio/
│
├── scripts/
│   ├── database/
│   │   ├── bootstrap/
│   │   └── tools/
│   ├── storage/
│   │   └── minio/
│   ├── seed/
│   │   └── run-development-seed.sh
│   │
│   └── benchmark/
│       ├── n1.md
│       ├── index.md
│       ├── pagination.md
│       ├── csv.md
│       └── batch.md
│
├── docs/
│   ├── architecture/
│   ├── uml/
│   ├── benchmark/
│   └── demo-script.md
│
├── .env.example
├── Directory.Build.props
├── LmsMini.sln
└── README.md
```

---
# 11. Cấu trúc thư mục chi tiết cho Course Service

```text
CourseService.Domain/
│
├── Entities/
│   ├── Course.cs
│   ├── Lesson.cs
│   ├── Enrollment.cs
│   ├── LessonProgress.cs
│   └── MediaObject.cs
│
├── Enums/
│   ├── CourseStatus.cs
│   ├── ProgressStatus.cs
│   └── MediaType.cs
│
├── ValueObjects/
│
├── Events/
│
└── Exceptions/
```

```text
CourseService.Application/
│
├── Abstractions/
│   ├── Persistence/
│   │   ├── ICourseRepository.cs
│   │   └── IUnitOfWork.cs
│   │
│   ├── Clients/
│   │   └── IMediaServiceClient.cs
│   │
│   └── Clock/
│       └── IDateTimeProvider.cs
│
├── Courses/
│   ├── Commands/
│   │   ├── CreateCourse/
│   │   └── UpdateCourse/
│   │
│   └── Queries/
│       ├── GetCourse/
│       ├── GetCourses/
│       ├── GetCoursesCursor/
│       └── ExportCourses/
│
├── Lessons/
│
├── Progress/
│
├── MediaUsages/
│
├── DTOs/
├── Validators/
└── Common/
```

```text
CourseService.Infrastructure/
│
├── Persistence/
│   ├── CourseDbContext.cs
│   ├── Configurations/
│   ├── Repositories/
│   └── Migrations/
│
├── Clients/
│   ├── MediaServiceClient.cs
│   └── StudentServiceClient.cs
│
└── DependencyInjection.cs
```

```text
CourseService.Api/
│
├── Controllers/
│   ├── CoursesController.cs
│   ├── LessonsController.cs
│   └── CourseMediaUsagesController.cs
│
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── CorrelationIdMiddleware.cs
│
├── Contracts/
│   ├── Requests/
│   └── Responses/
│
├── Extensions/
├── Program.cs
└── appsettings.json
```

---
# 12. Cấu trúc Media Service

```text
MediaService.Domain/
│
├── Entities/
│   ├── MediaObject.cs
│   └── MediaUsage.cs
│
└── Enums/
    ├── MediaType.cs
    ├── MediaOwnerService.cs
    ├── MediaOwnerType.cs
    └── MediaUsageType.cs
```

```text
MediaService.Application/
│
├── Media/
│   ├── Commands/
│   │   ├── UploadMedia/
│   │   ├── DeleteMedia/
│   │   └── CreateMediaUsage/
│   ├── Queries/
│   │   ├── GetMediaMetadata/
│   │   ├── GetMediaContent/
│   │   └── GetMediaUsages/
│
├── Abstractions/
│   ├── Storage/
│   │   └── IObjectStorage.cs
│   └── Persistence/
│       └── IMediaRepository.cs
│
└── Validators/
```

```text
MediaService.Infrastructure/
│
├── Persistence/
│   ├── MediaDbContext.cs
│   ├── Repositories/
│   └── Migrations/
│
└── Storage/
    └── MinioObjectStorage.cs
```

```text
MediaService.Api/
│
├── Controllers/
│   ├── MediaController.cs
│   └── MediaUsagesController.cs
│
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── CorrelationIdMiddleware.cs
│
└── Program.cs
```

---
# 13. Cấu trúc Notification Service

```text
NotificationService.Domain/
│
├── Entities/
│   ├── NotificationJob.cs
│   ├── NotificationJobItem.cs
│   └── Notification.cs
│
└── Enums/
    ├── NotificationJobStatus.cs
    ├── NotificationItemStatus.cs
    └── NotificationReadStatus.cs
```

```text
NotificationService.Application/
│
├── Jobs/
│   ├── Commands/
│   │   └── CreateBroadcastJob/
│   │
│   └── Queries/
│       ├── GetJob/
│       └── GetFailedItems/
│
├── Abstractions/
│   ├── IUserProvider.cs
│   ├── INotificationSender.cs
│   └── IMediaServiceClient.cs
│
└── DTOs/
```

```text
NotificationService.Infrastructure/
│
├── Persistence/
│├── NotificationDbContext.cs
│├── Repositories/
│
├── Clients/
│   ├── MediaServiceClient.cs
│   └── StudentServiceClient.cs
│
└── Senders/
    └── FakeNotificationSender.cs
```

---
