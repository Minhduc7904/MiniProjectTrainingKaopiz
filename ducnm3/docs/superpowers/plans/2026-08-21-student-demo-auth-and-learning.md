# Student demo auth và learning flow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cung cấp Student register/login/me, ghi danh và complete lesson
progress với identity demo qua header, cùng frontend Student route guard và
localStorage tách biệt Admin/Student.

**Architecture:** Student Service bổ sung use case write/read trên bảng
`students`; Course Service dùng bảng Enrollment/LessonProgress sẵn có và actor
header đã chuẩn hóa. Frontend dùng scoped actor storage và scoped HTTP clients;
Student guard dùng `/student/loading` làm bootstrap `me` trước mọi trang Student.

**Tech Stack:** ASP.NET Core Minimal API, Clean Architecture, EF Core/MySQL,
NUnit/TestServer/Testcontainers, React 19, React Router, Redux Toolkit, Axios,
Vitest.

---

## File map

- Modify shared route/header actor constants and route tests in
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/` only where a new
  public contract is shared.
- Create Student Application features under
  `backend/Services/Student/StudentService.Application/Features/Auth/`;
  create API contracts/endpoints under `StudentService.Api/Contracts/Auth/` and
  `StudentService.Api/Endpoints/Auth/`; extend the existing EF repository.
- Create Course Application features under
  `CourseService.Application/UseCases/Enrollments/` and
  `CourseService.Application/UseCases/LessonProgresses/`; add endpoint contracts
  under `CourseService.Api/Contracts/Learning/` and routes under
  `CourseService.Api/Endpoints/Learning/`; implement EF persistence in
  `CourseService.Infrastructure/Persistence/Repositories/`.
- Update existing service UnitTests, ComponentTests and IntegrationTests; do not
  create a migration because current V001 schema already has all required
  columns, foreign keys and unique indexes.
- Add scoped frontend auth modules under `frontend/lms-web/src/auth/` and
  `src/pages/student-auth/`, then prefix current admin routes in
  `src/constants/appRoutes.js` and `src/app/router.jsx`.
- Create one API doc and one business flow for each of the five endpoints; update
  architecture/test catalogs and Postman.

### Task 1: Lock contracts and shared route tests

**Files:**
- Modify: `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`
- Modify: `backend/Services/Student/StudentService.UnitTests/ApiRoutesTests.cs`
- Modify: `docs/architecture/backend/student-service/details/api.md`
- Modify: `docs/architecture/backend/course-service/details/api.md`

- [ ] **Step 1: Write failing route-contract tests** for `Auth.Register`,
  `Auth.Login`, `Auth.Me`, course enrollment and complete-progress service/public
  paths using fixed UUIDs.
- [ ] **Step 2: Run the Student and Course unit-test filters** and verify the
  failure is caused by missing route constants.
- [ ] **Step 3: Add only the constants/path builders required by the five API
  contracts**; retain every existing route.
- [ ] **Step 4: Re-run focused tests** and refactor duplicated route formatting
  only after green.

### Task 2: Implement Student register/login/me using TDD

**Files:**
- Create: `backend/Services/Student/StudentService.Application/Features/Auth/Register/*`
- Create: `backend/Services/Student/StudentService.Application/Features/Auth/Login/*`
- Create: `backend/Services/Student/StudentService.Application/Features/Auth/GetMe/*`
- Modify: `StudentService.Application/DependencyInjection.cs`
- Modify: `StudentService.Infrastructure/Persistence/EfStudentRepository.cs`
- Modify: `StudentService.Infrastructure/DependencyInjection.cs`
- Create: `StudentService.Api/Contracts/Auth/*.cs`
- Create: `StudentService.Api/Endpoints/Auth/*.cs`
- Modify: `StudentService.Api/Program.cs`
- Test: `StudentService.UnitTests/Auth/*`, `StudentService.ComponentTests/Endpoints/*`,
  `StudentService.IntegrationTests/Persistence/*`

- [ ] **Step 1: Add failing unit tests** for valid registration, normalized
  duplicate email, valid active login, missing login, inactive login, and `me`
  profile lookup/cancellation.
- [ ] **Step 2: Run only the new Student fixture** and confirm failures are
  missing types/behavior, not test setup failures.
- [ ] **Step 3: Implement minimal Application contracts, validators, handlers,
  error codes and repository ports**. Registration creates ACTIVE Student;
  login/me only return an ACTIVE Student.
- [ ] **Step 4: Add failing TestServer tests** for register `201`/`409`, login
  `200`/`404`/`403`, `me` `200`, malformed actor `400`, non-Student actor `403`
  and safe error envelopes.
- [ ] **Step 5: Add API request/response mapping and register endpoints in
  `Program.cs`**, using the shared actor filter for `me` and `no-store` for this
  identity-dependent GET.
- [ ] **Step 6: Implement EF methods and integration tests** against MySQL
  Testcontainer, applying production V001 migration and asserting unique email
  plus ACTIVE status persistence.
- [ ] **Step 7: Run focused unit/component/integration tests**, then format the
  Student test projects.

### Task 3: Implement enrollment and complete progress using TDD

**Files:**
- Create: `backend/Services/Course/CourseService.Application/Repositories/ILearningCommandRepository.cs`
- Create: `backend/Services/Course/CourseService.Application/UseCases/Enrollments/Create/*`
- Create: `backend/Services/Course/CourseService.Application/UseCases/LessonProgresses/Complete/*`
- Modify: `CourseService.Application/DependencyInjection.cs`
- Create: `CourseService.Infrastructure/Persistence/Repositories/EfLearningCommandRepository.cs`
- Modify: `CourseService.Infrastructure/DependencyInjection.cs`
- Create: `CourseService.Api/Contracts/Learning/*.cs`
- Create: `CourseService.Api/Endpoints/Learning/*.cs`
- Modify: `CourseService.Api/Program.cs`
- Test: `CourseService.UnitTests/Learning/*`, `CourseService.ComponentTests/Endpoints/*`,
  `CourseService.IntegrationTests/Persistence/*`

- [ ] **Step 1: Write failing unit tests** for PUBLISHED-course enrollment,
  missing/unpublished Course, duplicate enrollment, missing enrollment,
  Lesson-Course mismatch, first completion and repeated completion.
- [ ] **Step 2: Run the focused Course tests** and verify RED state.
- [ ] **Step 3: Add minimal handlers/ports/error codes**: obtain Student ID only
  from `ActorContext`, reject conditions explicitly, and make repeated complete
  return existing progress without changing its original `completedAt`.
- [ ] **Step 4: Write failing TestServer tests** for Student-only access,
  success envelopes, no request body, `403`, `404`, `409` and safe errors.
- [ ] **Step 5: Map endpoints with `RequireActor(ActorAccess.Student)` and
  production route constants**, then register them in Course `Program.cs`.
- [ ] **Step 6: Implement EF command repository and MySQL integration tests**
  for `uq_enrollments_course_id_student_id`,
  `uq_lesson_progresses_lesson_id_student_id`, PUBLISHED check and idempotent
  completion persistence.
- [ ] **Step 7: Run focused Course unit/component/integration tests** and
  format each touched project.

### Task 4: Document and expose all backend contracts

**Files:**
- Create: five endpoint docs under `docs/api/student-service/endpoints/` and
  `docs/api/course-service/endpoints/`
- Create: five matching flows under `docs/business-flows/students/` and
  `docs/business-flows/course-learning/`
- Modify: `postman/MiniProjectKaopiz.postman_collection.json`
- Modify: `docs/tests/student-service/{unit,component,integration}.md`
- Modify: `docs/tests/course-service/{unit,component,integration}.md`

- [ ] **Step 1: Document one-to-one API contract and business flow for every
  endpoint**, including headers, envelope examples, error codes, DB reads/writes
  and demo-security limitation.
- [ ] **Step 2: Add Postman requests using Gateway paths and variables**;
  include duplicate enrollment and repeated-complete verification.
- [ ] **Step 3: Validate Postman JSON**, update test catalogs to actual test
  names and verify cross-links.

### Task 5: Separate frontend actors and prefix admin routes

**Files:**
- Modify: `frontend/lms-web/src/constants/storage.js`
- Modify: `frontend/lms-web/src/auth/actorStorage.js`
- Modify: `frontend/lms-web/src/api/httpClient.js`
- Create: `frontend/lms-web/src/auth/studentAuthStorage.js`
- Create: `frontend/lms-web/src/api/studentAuthApi.js`
- Modify: `frontend/lms-web/src/constants/appRoutes.js`
- Modify: `frontend/lms-web/src/app/router.jsx`
- Modify: `frontend/lms-web/src/main.jsx`
- Test: `frontend/lms-web/src/auth/*.test.js`, `src/constants/appRoutes.test.js`

- [ ] **Step 1: Write failing Vitest tests** proving admin initialization never
  creates a Student actor, Student clear never removes Admin actor, and all
  existing app routes resolve below `/admin`.
- [ ] **Step 2: Run those tests**, confirm RED, then implement separate keys and
  scoped actor readers/writers.
- [ ] **Step 3: Split HTTP client creation or request configuration by actor
  scope** so admin APIs receive only Admin headers and Student auth request
  does not accidentally inherit an Admin header.
- [ ] **Step 4: Prefix every existing SPA route/menu/match route with `/admin`;
  redirect `/` to the selected admin landing page without changing Gateway API
  constants.**
- [ ] **Step 5: Re-run focused frontend tests** and refactor only duplicated
  actor/header construction.

### Task 6: Add Student auth pages, loading guard and logout

**Files:**
- Create: `frontend/lms-web/src/features/studentAuth/*`
- Create: `frontend/lms-web/src/hooks/studentAuth/*`
- Create: `frontend/lms-web/src/pages/student-auth/RegisterPage.jsx`
- Create: `frontend/lms-web/src/pages/student-auth/LoginPage.jsx`
- Create: `frontend/lms-web/src/pages/student-auth/LoadingPage.jsx`
- Create: `frontend/lms-web/src/pages/student-auth/LogoutPage.jsx`
- Create: `frontend/lms-web/src/auth/StudentRouteGuard.jsx`
- Modify: `frontend/lms-web/src/app/router.jsx`
- Modify: `frontend/lms-web/src/app/store.js`
- Modify: `docs/architecture/frontend/architecture.md`
- Test: `frontend/lms-web/src/auth/StudentRouteGuard.test.jsx`,
  `frontend/lms-web/src/pages/student-auth/*.test.jsx`

- [ ] **Step 1: Write failing component tests** for absent actor → login,
  unverified actor → loading with `returnTo`, `me` success → return destination,
  `me` `400/403/404` → clear Student actor + login, and logout preserving Admin
  actor.
- [ ] **Step 2: Run the focused tests** and confirm the expected RED failures.
- [ ] **Step 3: Implement a Redux auth state and API/hook layer**. Only
  `/student/loading` calls `me`; guard must not issue concurrent duplicate
  requests.
- [ ] **Step 4: Implement Register/Login forms using existing UI tokens and
  native accessible inputs**, save only `{ type: 'STUDENT', id }`, and replace
  navigation to loading. Implement LogoutPage as an effect that clears only
  Student storage then replaces to login.
- [ ] **Step 5: Add routes and guard nesting** so public auth pages remain
  reachable, loading requires stored actor, and protected Student routes cannot
  render before verification.
- [ ] **Step 6: Re-run focused frontend tests**, then `npm run lint`,
  `npm test`, and `npm run build` in `frontend/lms-web`.

### Task 7: Full verification

**Files:**
- Modify only if verification exposes a scoped defect.

- [ ] **Step 1: Run** `dotnet build backend/Lms.sln -m:1`.
- [ ] **Step 2: Run** `dotnet test backend/Lms.sln -m:1`; run Docker-backed
  integration projects only after `docker info` passes.
- [ ] **Step 3: Run** `dotnet format backend/Lms.sln --verify-no-changes`.
- [ ] **Step 4: Run** frontend `npm run lint`, `npm test`, `npm run build`.
- [ ] **Step 5: Validate Postman JSON and review documentation/API/flow/test
  catalog coverage against this design.**
