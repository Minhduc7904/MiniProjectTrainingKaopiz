# Testing Guide

## Framework and organization

All backend tests use NUnit. Test projects are named after their owning component and remain beside that component:

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

Do not create empty test projects. Add a service test project with the first behavior owned by that service.

## Detailed test catalogue

The executable test catalogue is organized in
[`../tests/README.md`](../tests/README.md), then by owning service or shared
component and test type. Each page documents the source test name, setup,
input/dependency state, expected result, and exact pass condition.

## Run tests

Run all backend tests:

```bash
dotnet test backend/Lms.sln -m:1
```

Run one service:

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

## Test types

| Type | Scope | External dependencies |
| --- | --- | --- |
| Unit | Domain, Application, deterministic Infrastructure behavior | None; mock or fake interfaces |
| Component | Middleware, minimal API endpoints, response envelopes | `TestServer`, no network or database |
| Service integration | Storage adapters, API, SQL migration, repositories, scaffolded DbContext | Isolated dependency containers |
| Gateway integration | YARP prefixes, downstream errors, Swagger proxying | TestServer and fake downstream HTTP handler |
| Cross-service integration | Service boundaries through Gateway | Docker Compose or dedicated Testcontainers |
| End-to-end | Browser workflow across frontend and backend | Playwright plus isolated system stack |

## Adding a new test

1. Put business-rule tests in `<Service>Service.UnitTests`.
2. Add endpoint or middleware tests to the owning component test project.
3. Add a service integration project only when the change needs real MySQL, SQL migration, or generated DbContext behavior.
4. Keep cross-service tests outside an individual service, in root `tests/`.
5. Every test must create its own data and clean up through its isolated test environment.

## Media Service storage tests

`MediaService.UnitTests` covers storage option validation, bucket mapping, UTC
object-key generation, upload request validation, database cancellation, and
all four database/MinIO health combinations.

`MediaService.IntegrationTests` starts an isolated MinIO Testcontainer. It
creates all five buckets and verifies upload, existence, download, metadata,
delete, category mapping, and storage health without depending on the
developer's Docker Compose stack. Docker must be running to execute this
project.

## Scheduler Service tests

`SchedulerService.UnitTests` currently verifies that
`SchedulerDatabaseHealthProbe` propagates request cancellation. Database
availability response mapping is covered by the shared presentation component
tests because Scheduler uses the common `MapDatabaseHealthEndpoint` mapping.
No job execution tests exist yet: the Worker intentionally has no polling,
claiming, CRON parsing, or handler loop in this phase.

## Data Seeder tests

`Lms.DataSeeder.UnitTests` covers deterministic UUID/data generation,
relationship ranges, exact plan calculation and Development safety guards.

`Lms.DataSeeder.IntegrationTests` starts isolated Student and Course MySQL 8.4
containers, applies the real V001 migrations and verifies seed counts,
relationship uniqueness, resume idempotency and non-empty database rejection.
The test never uses local Compose databases.
