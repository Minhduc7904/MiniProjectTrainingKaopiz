# Thiết kế refactor Student Service theo convention Media Service

## Mục tiêu

Chuẩn hóa toàn bộ Student Service theo cấu trúc/layer/convention đang dùng ở
Media Service, trong khi giữ nguyên database schema và toàn bộ HTTP contract
Student hiện có: list, detail, register, login và `me`.

## Không đổi hành vi

- Giữ route, HTTP method, request payload, response envelope, status code và
  error code đã công bố.
- Giữ bảng `students`, migration V001 và dữ liệu hiện có; không tạo migration.
- Giữ actor-header demo cho `GET /auth/me`; không thêm JWT, cookie hay session.
- Không copy MinIO, thumbnail, worker, outbox hoặc dependency đặc thù Media.

## Cấu trúc đích

```text
StudentService.Domain/
  Constants/StudentStatuses.cs
  Entities/Student.cs

StudentService.Application/
  Common/Errors/{StudentErrors,StudentErrorCodes,StudentApplicationException}.cs
  Repositories/{IStudentRepository,IStudentQueryRepository}.cs
  UseCases/
    Students/GetList/{GetStudentsQuery,GetStudentsHandler,GetStudentsResult,StudentListItem}.cs
    Students/GetById/{GetStudentByIdQuery,GetStudentByIdHandler,GetStudentByIdResult}.cs
    Auth/Register/{RegisterStudentCommand,RegisterStudentHandler,RegisterStudentResult}.cs
    Auth/Login/{LoginStudentCommand,LoginStudentHandler,LoginStudentResult}.cs
    Auth/GetMe/{GetCurrentStudentQuery,GetCurrentStudentHandler,GetCurrentStudentResult}.cs

StudentService.Infrastructure/
  Persistence/Mappers/StudentPersistenceMapper.cs
  Persistence/Repositories/{EfStudentQueryRepository,EfStudentRepository}.cs

StudentService.Api/
  Contracts/Auth/{RegisterStudentRequest,LoginStudentRequest,StudentAuthResponse,StudentProfileResponse}.cs
  Contracts/Students/{StudentResponse,StudentListResponse}.cs
  Mappers/StudentResponseMapper.cs
  Endpoints/Auth/{Register,Login,GetMe}/...Endpoint.cs
  Endpoints/Students/{GetList,GetById}/...Endpoint.cs
```

## Ranh giới layer

Domain sở hữu `Student` và `StudentStatuses`. Application nhận command/query,
áp business rule và chỉ phụ thuộc repository interface/Domain. Infrastructure
map scaffolded EF entity sang Domain/Application result qua persistence mapper,
đồng thời tách read query khỏi write repository. API bind HTTP, gọi handler,
map Application result bằng response mapper và không truy cập DbContext.

## Chuẩn errors và constants

`StudentErrors` là điểm tạo duy nhất cho validation, not-found, duplicate email
và inactive Student; error code/message nằm cùng `StudentErrorCodes`. Không giữ
exception/error rải ở feature folder. API dùng shared `ApiErrorCodes`,
`ApiHeaderNames`, `ActorHeaderTypes` và `ApiRoutes` thay vì literal.

## Kiểm thử và tài liệu

Di chuyển test theo owner thực tế, không đổi case: Unit test handler/query/map,
component test endpoint/actor/error envelope, integration test EF mapper/query
trên MySQL Testcontainer. Cập nhật test catalog, architecture detail và API docs
nếu đường dẫn source được nêu trong tài liệu; Postman không đổi contract.

## Tiêu chí hoàn thành

- Không còn `Features/Students` hoặc `Features/Auth/StudentAuth.cs` kiểu gộp.
- Mỗi endpoint có endpoint class, request/response contract và mapper riêng.
- Mapping EF chỉ nằm dưới `Infrastructure/Persistence/Mappers`.
- Build/test đích pass; contract snapshot/component tests chứng minh behavior
  không drift.
