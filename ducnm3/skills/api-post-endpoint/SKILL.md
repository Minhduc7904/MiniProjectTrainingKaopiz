---
name: api-post-endpoint
description: Tạo hoặc thay đổi một endpoint POST create hoặc action ASP.NET Core theo Clean Architecture, gồm Location, idempotency, tài liệu và kiểm thử.
---

# API POST endpoint

## Khi sử dụng

Áp dụng riêng cho một endpoint `POST`. Trước tiên phân loại endpoint là
**create đồng bộ** hoặc **action bất đồng bộ**; không trộn hai contract trong một
endpoint và không gộp `GET`, `PUT`, `PATCH` hay `DELETE` vào skill này.

## Tài liệu bắt buộc

Trước khi sửa code:

1. Đọc toàn bộ [`reference.md`](reference.md) để xác định project, contract
   `201`/`202`, transaction và idempotency convention.
2. Đọc toàn bộ [`template.md`](template.md), chọn đúng nhánh create hoặc action
   và dùng checklist/code skeleton.
3. Đọc `rules/documentation-language.md`, `rules/code-quality.md`,
   `rules/testing.md`, `rules/api-documentation.md` và
   `rules/workflow-skills.md`.
4. Đọc API, business flow, database, messaging, architecture và implementation
   liên quan.
5. Đọc riêng `skills/test-unit/`, `skills/test-component/` và
   `skills/test-integration/`; đọc migration skill nếu đổi schema/index.

Không triển khai khi chưa chốt resource/action semantics, success status,
`Location`, transaction/side effects và idempotency policy.

## Quy trình

### 1. Chốt contract và tài liệu trước

- Xác định owner service, direct/public path, auth/ownership, content type,
  request schema, validation, business preconditions và side effects.
- Chọn đúng một success contract:
  - **create đồng bộ:** `201 Created`, response envelope của resource/result và
    `Location` trỏ đến canonical GET của resource mới;
  - **action bất đồng bộ:** `202 Accepted`, response envelope chứa operation/job
    reference và `Location` trỏ đến endpoint trạng thái operation.
- `Location` phải là path client có thể dùng, ưu tiên public Gateway path theo
  API contract; không trỏ vào internal database/storage.
- POST không tự nhiên idempotent. Chốt `Idempotency-Key` hoặc business key,
  phạm vi key, TTL, payload fingerprint, replay response, concurrent duplicate
  behavior và conflict khi cùng key khác payload.
- Tạo/cập nhật đúng **một** API doc tại
  `docs/api/<owning-service>/endpoints/post-<resource-or-action>.md`.
- Tạo/cập nhật đúng **một** business-flow file 1:1 tại
  `docs/business-flows/<domain>/post-<resource-or-action>.md`.

### 2. Route và constants dùng chung

- Thêm route, canonical resource/status path builder vào
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`.
- Tái sử dụng shared error/header/path constants; thêm
  `ApiHeaderNames.IdempotencyKey` nếu contract dùng header và chưa có.
- Business error/idempotency conflict codes thuộc Application owner service.
- Test route, `Location` path builder và shared constants.

### 3. Application feature

- Tạo feature riêng:
  `<Service>.Application/Features/<Resources>/Create/` hoặc
  `<Service>.Application/Features/<Actions>/Start/`.
- Tách command, result, validator, handler, abstraction và public type thành file
  riêng theo convention.
- Validate payload/identity/idempotency key; kiểm tra authorization, not-found,
  conflict, state transition và invariant trước side effect.
- Handler điều phối transaction qua abstraction. Với `202`, persist operation
  trước khi dispatch message/job và dùng outbox hoặc consistency strategy rõ.
- Idempotency store phải atomically claim key và lưu fingerprint/result/status;
  replay cùng payload trả cùng logical result, không lặp write/event.
- Application không phụ thuộc ASP.NET Core, EF Core, MySQL hoặc project Api.

### 4. Infrastructure adapter

- Implement repository/unit-of-work/idempotency/outbox adapters trong
  `<Service>.Infrastructure`.
- Bảo vệ uniqueness/concurrency bằng database constraint, conditional write và
  map duplicate race sang expected conflict/replay.
- Create đồng bộ ghi resource + idempotency result trong transaction.
- Action `202` ghi operation + outbox/idempotency trong transaction; không báo
  accepted nếu durable handoff chưa được bảo đảm theo contract.
- Đăng ký adapter. Migration/schema thay đổi phải theo migration workflow.

### 5. API endpoint

- Tạo request/response contracts và mapper tại project Api.
- Dùng `MapPost(ApiRoutes....)`, `.Accepts<T>(contentType)`, operation name, tag
  và `.Produces...` cho mọi status thực tế.
- Parse `Idempotency-Key`/route/header an toàn, map request sang command và
  truyền `CancellationToken`; endpoint không chứa business logic.
- Set `Location` từ shared path builder.
- Trả `ApiResponseFactory.Success(...)` với `201` hoặc `202` đã chốt.
- Đăng ký endpoint trong `Program.cs`.

### 6. Error envelope

- Expected errors đi qua shared middleware/application exception:
  `error.code`, safe `error.message`, `error.details`, `meta.traceId`.
- Xem xét `400` validation, `401`, `403`, `404` dependency/resource,
  `409` business/idempotency conflict, `413`/`415` nếu payload áp dụng và `5xx`.
- Không để raw duplicate-key, broker, SQL, credential, object key hoặc stack
  trace lộ ra.

### 7. Coverage

- **Unit:** validation, invariant/state transition, found/not-found/conflict,
  idempotency first/replay/key-with-different-payload và side-effect count.
- **Component:** TestServer binding/content type/header, `201` hoặc `202`,
  `Location`, standard envelope/error, auth và replay cùng key.
- **Integration:** database/container thật cho transaction, unique race,
  rollback, idempotency persistence/replay; với `202`, operation + outbox/durable
  dispatch và retry không tạo job/message trùng.
- Test deterministic, cô lập; không thay component/integration bằng mock-only
  unit test.

### 8. Postman và kiểm tra cuối

- Cập nhật `postman/MiniProjectKaopiz.postman_collection.json` trong đúng service
  với POST request, full payload, content type/auth/idempotency headers, success
  test, `Location`, validation/not-found/conflict và replay cases.
- Với `201`, lưu resource ID/Location. Với `202`, lưu operation ID/Location để
  polling bằng request riêng đã thuộc collection.
- Chạy test feature/project, integration test, rồi
  `dotnet build backend/Lms.sln -m:1`,
  `dotnet test backend/Lms.sln -m:1`,
  `dotnet format backend/Lms.sln --verify-no-changes`.
- Đối chiếu code, đúng một API doc, business flow 1:1 và Postman.

## Tiêu chí hoàn thành

- Endpoint chỉ dùng POST và chọn đúng một contract `201` create hoặc `202`
  action; `Location` hợp lệ.
- Idempotency, transaction, concurrency và side effects nhất quán giữa code,
  tests, docs và Postman.
- Dependency vẫn đi `Api -> Application <- Infrastructure`; Application phụ
  thuộc Domain và abstraction.
