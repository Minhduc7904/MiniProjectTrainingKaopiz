# Student Service Media-Style Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Chuẩn hóa cấu trúc Student Service giống convention Media Service mà không đổi HTTP contract hay schema.

**Architecture:** Tách Domain entity/constants, Application errors/repositories/use cases, Infrastructure persistence mapper/read-write repositories, và API contracts/mappers/endpoint theo use case. EF scaffolded entity chỉ tồn tại ở Infrastructure.

**Tech Stack:** ASP.NET Core, Clean Architecture, EF Core/MySQL, NUnit/TestServer/Testcontainers.

---

### Task 1: Lock existing contracts with tests

**Files:**
- Modify: `StudentService.UnitTests/ApiRoutesTests.cs`
- Create: `StudentService.UnitTests/Architecture/StudentLayerDependencyTests.cs`

- [ ] Write/refine route and dependency tests; run them before moving source files.
- [ ] Preserve all `ApiRoutes`, request/response JSON and status codes.

### Task 2: Establish Domain and Application boundaries

**Files:**
- Create: `StudentService.Domain/Constants/StudentStatuses.cs`
- Create: `StudentService.Domain/Entities/Student.cs`
- Create: `StudentService.Application/Common/Errors/*`
- Create: `StudentService.Application/Repositories/*`
- Move/create: all list, detail and auth use-case files beneath `Application/UseCases/`
- Modify: `StudentService.Application/DependencyInjection.cs`

- [ ] Write failing handler/mapping unit tests, then move one use case at a time.
- [ ] Replace feature-local errors and generic auth aggregate with one command/query/result/handler per use case.
- [ ] Run Student unit tests after each slice.

### Task 3: Split persistence and introduce mapper

**Files:**
- Create: `StudentService.Infrastructure/Persistence/Mappers/StudentPersistenceMapper.cs`
- Create: `StudentService.Infrastructure/Persistence/Repositories/EfStudentRepository.cs`
- Create: `StudentService.Infrastructure/Persistence/Repositories/EfStudentQueryRepository.cs`
- Modify: `StudentService.Infrastructure/DependencyInjection.cs`
- Remove: legacy `Persistence/EfStudentRepository.cs` after all registrations/tests use replacement.

- [ ] Add mapper tests first.
- [ ] Keep `StudentDbContext` and V001 schema unchanged.
- [ ] Verify integration query tests use production adapter.

### Task 4: Split API contracts, response mapper and endpoints

**Files:**
- Create: `StudentService.Api/Contracts/Auth/**`
- Create: `StudentService.Api/Contracts/Students/**`
- Create: `StudentService.Api/Mappers/StudentResponseMapper.cs`
- Create: `StudentService.Api/Endpoints/Auth/{Register,Login,GetMe}/**`
- Create: `StudentService.Api/Endpoints/Students/{GetList,GetById}/**`
- Modify: `StudentService.Api/Program.cs`
- Remove: legacy aggregate endpoint files only after component tests are green.

- [ ] Write endpoint component tests that preserve existing envelopes and actor behavior.
- [ ] Move binding/mapping only; keep Application free of ASP.NET Core types.

### Task 5: Verify and update documentation

**Files:**
- Modify: `docs/architecture/backend/student-service/**`
- Modify: `docs/tests/student-service/{unit,component,integration}.md`

- [ ] Run Student unit/component/integration tests, `dotnet build` and format verification.
- [ ] Record any pre-existing verification failure separately from this refactor.
