# Course CSV Export Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cung cấp CSV export Course UTF-8 BOM theo chunk keyset, không materialize toàn bộ dữ liệu hoặc CSV string.

**Architecture:** Application định nghĩa query, row và streaming repository contract; Infrastructure truy vấn EF theo keyset `createdAt,id`; API viết BOM/header/row trực tiếp vào response. CSV encoding là utility Application thuần để unit-test.

**Tech Stack:** ASP.NET Core Minimal API, EF Core/MySQL, NUnit/TestServer.

---

### Task 1: Application export contract và CSV encoder

**Files:**
- Create: `backend/Services/Course/CourseService.Application/UseCases/Courses/Export/*.cs`
- Modify: `backend/Services/Course/CourseService.Application/Repositories/ICourseListRepository.cs`
- Modify: `backend/Services/Course/CourseService.Application/DependencyInjection.cs`
- Test: `backend/Services/Course/CourseService.UnitTests/Application/Courses/Export/*.cs`

- [ ] Viết test fail cho BOM/header, quote/comma/newline escaping và status normalize.
- [ ] Tạo `ExportCoursesQuery`, `CourseExportChunk`, `CourseExportRow`,
  `ExportCoursesHandler`, `CsvRowWriter`; contract repository trả tối đa một chunk.
- [ ] Chạy unit tests Course cho đến khi pass.

### Task 2: EF keyset adapter và route

**Files:**
- Modify: `backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfCourseListRepository.cs`
- Modify: `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`
- Create: `backend/Services/Course/CourseService.Api/Endpoints/Courses/Export/ExportCoursesEndpoint.cs`
- Modify: `backend/Services/Course/CourseService.Api/Program.cs`

- [ ] Viết component test fail cho `GET /api/courses/export`.
- [ ] Query `AsNoTracking`, apply status, keyset predicate và projection trước
  `Take(500)`; endpoint ghi UTF-8 BOM/header/row trực tiếp vào `Response.Body`.
- [ ] Chạy component tests; đảm bảo invalid status trả `400` JSON trước response CSV.

### Task 3: Tài liệu, Postman và verification

**Files:**
- Modify: `docs/api/course-service/endpoints/get-courses-export.md`
- Create: `docs/business-flows/courses/get-courses-export.md`
- Modify: `docs/tests/course-service/unit.md`
- Modify: `docs/tests/course-service/component.md`
- Modify: `docs/tests/README.md`
- Modify: `postman/MiniProjectKaopiz.postman_collection.json`

- [ ] Ghi contract/status/cancellation/consistency và URL Gateway trong docs.
- [ ] Thêm request CSV Postman cùng test Content-Type và BOM.
- [ ] Chạy `dotnet test` Unit/Component, `dotnet build` API và JSON validate collection.
