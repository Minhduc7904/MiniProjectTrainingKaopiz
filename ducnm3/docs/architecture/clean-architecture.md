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

# Shared API health and error handling

`BuildingBlocks.Contracts` owns framework-independent response DTOs, health-probe interfaces, error codes, header names, and status constants. `BuildingBlocks.Presentation` owns ASP.NET Core middleware, response mapping, and reusable endpoint mappings.

Each service Infrastructure implements `IDatabaseHealthProbe` with its own MySQL connection string and a `SELECT 1` query. API maps `GET /health`; a reachable API with an unavailable database returns `503 DATABASE_UNAVAILABLE`. API Gateway maps unreachable downstream services to `503 SERVICE_UNAVAILABLE`.

Domain and Application do not depend on ASP.NET Core, MySQL, or the presentation project.

## Test organization

- Shared API behavior lives in `BuildingBlocks.Presentation.Tests`, separated by `Middleware/`, `Endpoints/`, and `Gateway/`.
- Each service owns its `*Service.UnitTests` project beside its API, Application, Domain, and Infrastructure projects.
- Future integration tests use `*Service.IntegrationTests` beside their service and a real isolated MySQL container.
- Cross-service and end-to-end tests belong in the root `tests/` directory, not in an individual service.

# 10. Folder Structure tổng thể

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
│   │   └── Notification/
│   │       ├── NotificationService.Api/
│   │       ├── NotificationService.Application/
│   │       ├── NotificationService.Domain/
│   │       ├── NotificationService.Infrastructure/
│   │       └── NotificationService.UnitTests/
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
│   ├── database/
│   │   ├── bootstrap/
│   │   └── tools/
│   ├── storage/
│   │   └── minio/
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
