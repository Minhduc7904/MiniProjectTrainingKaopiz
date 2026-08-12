# Testing Guide

## Framework and organization

All backend tests use NUnit. Test projects are named after their owning component and remain beside that component:

```text
backend/
├── BuildingBlocks/BuildingBlocks.Presentation.Tests/
│   ├── Middleware/
│   ├── Endpoints/
│   └── Gateway/
└── Services/<Service>/
    └── <Service>Service.UnitTests/
```

Do not create empty test projects. Add a service test project with the first behavior owned by that service.

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
dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj
```

## Test types

| Type | Scope | External dependencies |
| --- | --- | --- |
| Unit | Domain, Application, deterministic Infrastructure behavior | None; mock or fake interfaces |
| Component | Middleware, minimal API endpoints, response envelopes | `TestServer`, no network or database |
| Service integration | API, SQL migration, repositories, scaffolded DbContext | Isolated MySQL Testcontainers database |
| Gateway integration | YARP prefixes, downstream errors, Swagger proxying | TestServer and fake downstream HTTP handler |
| Cross-service integration | Service boundaries through Gateway | Docker Compose or dedicated Testcontainers |
| End-to-end | Browser workflow across frontend and backend | Playwright plus isolated system stack |

## Adding a new test

1. Put business-rule tests in `<Service>Service.UnitTests`.
2. Add endpoint or middleware tests to the owning component test project.
3. Add a service integration project only when the change needs real MySQL, SQL migration, or generated DbContext behavior.
4. Keep cross-service tests outside an individual service, in root `tests/`.
5. Every test must create its own data and clean up through its isolated test environment.
