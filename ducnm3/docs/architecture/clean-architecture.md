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

`BuildingBlocks.Contracts` quản lý DTO response, health probe interface, error
code, HTTP header name và status constant. `BuildingBlocks.Presentation` quản lý
ASP.NET Core middleware, response mapping và endpoint mapping dùng chung.
`BuildingBlocks.Messaging.Abstractions` chứa interface COMMAND/EVENT không phụ
thuộc MassTransit; `BuildingBlocks.Messaging` và `BuildingBlocks.Http` là các
adapter Infrastructure cho RabbitMQ và typed QUERY client.

Tầng Infrastructure của mỗi service triển khai `IDatabaseHealthProbe` bằng
connection string MySQL riêng và truy vấn `SELECT 1`. `GET /health` kiểm tra cả
database và MassTransit bus; Media Service kiểm tra thêm MinIO. Database lỗi trả
`503 DATABASE_UNAVAILABLE`, broker hoặc nhiều dependency lỗi trả
`503 DEPENDENCY_UNAVAILABLE`.

Domain và Application không phụ thuộc vào ASP.NET Core, MySQL hoặc dự án trình bày.

Scheduler tuân theo bốn tầng tương tự và bổ sung `SchedulerService.Worker` làm
tiến trình nền độc lập. Worker đã host MassTransit và kết nối RabbitMQ, nhưng
chưa có consumer, polling, claiming, phân tích CRON hoặc job handler nghiệp vụ.
Siêu dữ liệu lập lịch nằm trong `lms_scheduler_db`; trạng thái người nhận và nội
dung notification nằm trong `lms_notification_db`.

## Tổ chức kiểm thử

- Hành vi API dùng chung nằm trong `BuildingBlocks.Presentation.Tests`, được phân chia theo `Middleware/`, `Endpoints/` và `Gateway/`.
- Naming, options, HTTP retry và correlation nằm trong
  `BuildingBlocks.Communication.UnitTests`; topology, COMMAND/EVENT fan-out,
  centralized retry và `_error` queue nằm trong
  `BuildingBlocks.Messaging.IntegrationTests`.
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
│   │   ├── BuildingBlocks.Http/
│   │   ├── BuildingBlocks.Messaging.Abstractions/
│   │   ├── BuildingBlocks.Messaging/
│   │   ├── BuildingBlocks.Communication.UnitTests/
│   │   ├── BuildingBlocks.Messaging.IntegrationTests/
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
│   │   │   ├── StudentService.UnitTests/
│   │   │   ├── StudentService.ComponentTests/
│   │   │   └── StudentService.IntegrationTests/
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
│       │   ├── app/
│       │   ├── api/
│       │   ├── constants/
│       │   ├── features/
│       │   ├── hooks/
│       │   ├── components/
│       │   └── pages/
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
├── Actors/
│   ├── ActorReference.cs
│   └── ActorTypes.cs
├── Media/
│   ├── MediaObjectStatuses.cs
│   └── MediaTypes.cs
└── Usages/
    ├── MediaOwnerServices.cs
    ├── MediaOwnerTypes.cs
    └── MediaUsageTypes.cs
```

```text
MediaService.Application/
├── Abstractions/
│   ├── Clients/
│   ├── Persistence/
│   └── Storage/
├── Actors/
├── Features/
│   ├── Media/
│   │   ├── GetContent/
│   │   └── Upload/
│   └── Usages/Create/
├── DependencyInjection.cs
├── MediaApplicationException.cs
├── MediaErrorCodes.cs
└── MediaErrors.cs
```

```text
MediaService.Infrastructure/
├── Clients/Student/
├── Database/Migrations/
├── Persistence/
│   ├── MediaDbContext.cs
│   ├── EfMediaRepository.cs
│   └── Scaffolded/
└── Storage/Minio/
```

```text
MediaService.Api/
├── Contracts/
│   ├── Requests/
│   └── Responses/
├── Endpoints/Media/
├── Mappers/
└── Program.cs
```

Mỗi public type trong feature Media/Student nằm ở file riêng. Application đăng
ký handler, validator và options qua `AddMediaApplication()`; Infrastructure chỉ
đăng ký EF, MinIO, HTTP client và health probe. Shared route/Student response
contract nằm trong `BuildingBlocks.Contracts`.

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
