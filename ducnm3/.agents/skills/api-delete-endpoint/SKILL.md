---
name: api-delete-endpoint
description: Tạo hoặc thay đổi một endpoint DELETE ASP.NET Core theo Clean Architecture, bao gồm deletion policy, side effects, tài liệu và kiểm thử.
---

# API DELETE endpoint

## Khi sử dụng

Áp dụng riêng cho một endpoint `DELETE`. Không dùng quy trình này cho `PUT`,
`PATCH` hoặc gộp nhiều HTTP method vào cùng thay đổi.

## Tài liệu bắt buộc

Trước khi sửa code:

1. Đọc toàn bộ [`reference.md`](reference.md) để xác định project, conventions và
   tiêu chí chọn soft/hard delete.
2. Đọc toàn bộ [`template.md`](template.md) và dùng checklist/code skeleton phù
   hợp.
3. Đọc `rules/documentation-language.md`, `rules/code-quality.md`,
   `rules/testing.md`, `rules/api-documentation.md` và
   `rules/workflow-skills.md`.
4. Đọc API doc, business flow, database/architecture liên quan, retention/audit
   rule và implementation hiện có.
5. Đọc riêng `.agents/skills/test-unit/`, `.agents/skills/test-component/` và
   `.agents/skills/test-integration/`; đọc `.agents/skills/database-migration/` nếu đổi schema,
   constraint hoặc index.

Không triển khai trước khi chốt soft hay hard delete, idempotency, hành vi khi
resource không tồn tại/đã xóa, conflict và toàn bộ side effects.

## Quy trình

### 1. Chốt contract và tài liệu trước

- Xác định owner service, `DELETE /api/<resources>/{id}`, authentication,
  authorization/ownership và business preconditions.
- Chọn có chủ đích:
  - **soft delete:** trạng thái/deleted timestamp, query visibility, restore,
    retention/purge và unique constraint behavior;
  - **hard delete:** cascade/restrict, audit/retention/legal requirement và khả
    năng khôi phục.
- Chốt idempotency cho request lặp và chọn hành vi status:
  - `204 No Content` khi xóa thành công;
  - `404` khi contract muốn phân biệt resource chưa tồn tại/đã xóa;
  - `409` khi dependency, active usage hoặc state cấm xóa.
- `204` phải có body rỗng. Ghi rõ side effects đồng bộ/bất đồng bộ, event/outbox,
  cleanup storage/cache/search và retry/failure behavior.
- Tạo/cập nhật đúng **một** API doc tại
  `docs/api/<owning-service>/endpoints/delete-<resource>.md`.
- Tạo/cập nhật đúng **một** business-flow file 1:1 tại
  `docs/business-flows/<domain>/<delete-resource>.md`, chỉ cho DELETE đang làm.

### 2. Route và constants dùng chung

- Thêm route/path builder vào
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`.
- Tái sử dụng shared route/header/status/error constants. Business error code
  thuộc Application của owner service.
- Không hard-code route/error string trong endpoint, adapter hoặc tests.
- Thêm route contract test khi thay đổi shared route.

### 3. Application feature

- Tạo feature riêng, ví dụ
  `<Service>.Application/Features/<Resources>/Delete/`.
- Tách `Delete<Resource>Command`, validator, handler và policy/result (nếu cần)
  theo convention public type/file.
- Validate ID/concurrency token; handler tải aggregate nếu policy cần, kiểm tra
  not-found/already-deleted/access/business state/dependencies.
- Với soft delete, chuyển state hợp lệ, lưu actor/time/reason nếu contract có và
  bảo đảm default query không trả record đã xóa.
- Với hard delete, kiểm tra relation/usage trước transaction và để database
  constraint là hàng rào cuối; map conflict an toàn.
- Request lặp không tạo thêm event, cleanup task hoặc audit record. Application
  không phụ thuộc ASP.NET Core, EF Core, MySQL hay storage SDK.

### 4. Infrastructure adapter

- Mở rộng Application abstraction và implement trong Infrastructure.
- Soft delete dùng conditional update (`id`, current state và optional version);
  hard delete dùng delete có điều kiện và chiến lược cascade/restrict đã document.
- Phân biệt `404`, already-deleted idempotent outcome và `409`; xử lý race giữa
  dependency check và delete bằng transaction/constraint.
- Ghi outbox cùng transaction nếu phát event. Cleanup external resource không
  được làm transaction database ở trạng thái nửa vời; dùng retryable job/event
  khi phù hợp.
- Đăng ký adapter trong `DependencyInjection.cs`; migration chỉ khi schema đổi.

### 5. API endpoint

- Dùng `MapDelete(ApiRoutes....)`, stable operation name/tag và
  `.Produces(StatusCodes.Status204NoContent)` cùng error metadata thực tế.
- Parse route/header/query an toàn, map command, truyền `CancellationToken`;
  không chứa business logic hoặc gọi DbContext/storage trực tiếp.
- Thành công trả `Results.NoContent()`. Expected error do shared middleware map.
- Đăng ký endpoint trong `Program.cs`.

### 6. Error envelope

- Mọi JSON error dùng `error.code`, safe `error.message`, `error.details`,
  `meta.traceId`.
- Tối thiểu xem xét `400` invalid ID/token, `401`, `403`, `404`, `409`
  in-use/dependency/state/concurrency và `5xx`.
- `204` không dùng success envelope vì không có body.
- Không lộ stack trace, SQL, credentials, relation internals hoặc storage key.

### 7. Coverage

- **Unit:** soft/hard policy, not-found/already-deleted behavior, idempotency,
  authorization, dependency/state conflict, concurrency và side-effect dedupe.
- **Component:** route/binding/header, `204` body rỗng, exact `404/409`, standard
  error envelope, auth và endpoint metadata.
- **Integration:** database thật cho soft-delete visibility hoặc hard-delete
  cascade/restrict, transaction/rollback, concurrent/repeated delete, outbox và
  cleanup scheduling.
- Tests deterministic, độc lập; component test với stub không thay integration.

### 8. Postman và kiểm tra cuối

- Cập nhật `postman/MiniProjectKaopiz.postman_collection.json`: DELETE request,
  variables/auth/concurrency header, success test khẳng định `204` + body rỗng,
  repeat-delete idempotency, `404` và `409`.
- Chạy test feature/project, integration test, `dotnet build`, toàn bộ
  `dotnet test`, `dotnet format --verify-no-changes`.
- Đối chiếu deletion policy, statuses và side effects giữa code/tests/API doc/
  business-flow 1:1/Postman trước bàn giao.

## Tiêu chí hoàn thành

- Soft/hard delete và idempotency được quyết định rõ, không suy ra ngầm.
- `204/404/409`, transaction và side effects nhất quán, có coverage đúng layer.
- Dependency giữ `Api -> Application <- Infrastructure`; không gộp method khác.
