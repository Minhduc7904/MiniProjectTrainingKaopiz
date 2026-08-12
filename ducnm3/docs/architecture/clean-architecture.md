# 9. Clean Architecture trong mỗi Microservice

Mỗi service giữ 4 layer:

```text
Domain
Application
Infrastructure
Api
```

Dependency:

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
 └── không phụ thuộc layer nào
```

---
# 10. Folder Structure tổng thể

```text
lms-mini/
│
├── backend/
│   │
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Contracts/
│   │   └── BuildingBlocks.Shared/
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
│   │   │   └── CourseService.Infrastructure/
│   │   │
│   │   ├── Student/
│   │   │   ├── StudentService.Api/
│   │   │   ├── StudentService.Application/
│   │   │   ├── StudentService.Domain/
│   │   │   └── StudentService.Infrastructure/
│   │   │
│   │   ├── Media/
│   │   │   ├── MediaService.Api/
│   │   │   ├── MediaService.Application/
│   │   │   ├── MediaService.Domain/
│   │   │   └── MediaService.Infrastructure/
│   │   │
│   │   └── Notification/
│   │       ├── NotificationService.Api/
│   │       ├── NotificationService.Application/
│   │       ├── NotificationService.Domain/
│   │       └── NotificationService.Infrastructure/
│   │
│   └── Workers/
│       └── NotificationWorker/
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
│   ├── seed/
│   │   ├── seed-10k.sql
│   │   ├── seed-100k.sql
│   │   └── seed-1m.sql
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
# 11. Folder Structure chi tiết cho Course Service

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
# 12. Media Service Structure

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
# 13. Notification Service Structure

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
