---
name: api-patch-endpoint
description: Tạo hoặc thay đổi một endpoint PATCH ASP.NET Core theo Clean Architecture, bao gồm contract partial update, triển khai, tài liệu và kiểm thử.
---

# API PATCH endpoint

## Khi sử dụng

Áp dụng riêng cho một endpoint `PATCH` cập nhật một phần tài nguyên. Không dùng
cho `PUT`, `DELETE` hoặc gộp nhiều HTTP method trong cùng quy trình.

## Tài liệu bắt buộc

Trước khi sửa code:

1. Đọc toàn bộ [`reference.md`](reference.md) để xác định đúng project,
   conventions và cách biểu diễn field presence.
2. Đọc toàn bộ [`template.md`](template.md) và dùng checklist/code skeleton phù
   hợp.
3. Đọc `rules/documentation-language.md`, `rules/code-quality.md`,
   `rules/testing.md`, `rules/api-documentation.md` và
   `rules/workflow-skills.md`.
4. Đọc API doc, business flow, database/architecture liên quan và implementation
   hiện có.
5. Đọc riêng `skills/test-unit/`, `skills/test-component/` và
   `skills/test-integration/`; đọc `skills/database-migration/` nếu đổi schema
   hoặc index.

Không triển khai trước khi contract phân biệt rõ field **absent** với field có
giá trị JSON `null`, allowed fields và concurrency behavior.

## Quy trình

### 1. Chốt contract và tài liệu trước

- Xác định owner service, `PATCH /api/<resources>/{id}`, media type được chấp
  nhận, authentication, authorization/ownership.
- Liệt kê allowlist field được cập nhật. Reject read-only/unknown/disallowed
  fields; không bind rồi âm thầm bỏ qua.
- Với từng field, ghi rõ:
  - absent: giữ nguyên;
  - present non-null: validate rồi cập nhật;
  - present null: clear nếu contract cho phép, ngược lại validation error.
- Chọn representation: JSON Merge Patch, JSON Patch, hoặc typed partial request
  có presence wrapper/custom converter. Không dùng nullable CLR property đơn
  thuần nếu cần phân biệt absent với explicit null.
- Chốt concurrency token (`If-Match`/ETag hoặc version), stale-token error và
  no-op behavior.
- Tạo/cập nhật đúng **một** API doc tại
  `docs/api/<owning-service>/endpoints/patch-<resource>.md`.
- Tạo/cập nhật đúng **một** business-flow file 1:1 tại
  `docs/business-flows/<domain>/<patch-resource>.md`, chỉ cho PATCH đang làm.

### 2. Route và constants dùng chung

- Thêm route/path builder vào
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`.
- Tái sử dụng shared `ApiErrorCodes`, header/status constants. Business error
  code thuộc Application của owner service.
- Không dùng magic path, header hoặc error string trong endpoint/adapter/test.
- Thêm route contract test khi shared route thay đổi.

### 3. Application feature

- Tạo feature riêng, ví dụ
  `<Service>.Application/Features/<Resources>/Patch/`.
- Tách command, field-presence type, validator, handler và result thành các public
  type/file rõ ràng theo convention.
- Validator từ chối patch rỗng nếu contract quy định, field không được phép,
  invalid null và invalid value; chỉ validate value của field có mặt.
- Handler tải aggregate, kiểm tra not-found/access/business preconditions và
  concurrency, rồi chỉ thay đổi field present.
- No-op patch phải có behavior được chốt; không cập nhật audit/version hoặc phát
  event nếu contract coi là không thay đổi.
- Side effect chỉ phát sinh từ state transition thực tế và phải an toàn khi retry.
- Application không phụ thuộc ASP.NET Core, JSON library, EF Core hoặc MySQL;
  Api chịu trách nhiệm parse patch document sang command trung lập.

### 4. Infrastructure adapter

- Mở rộng Application persistence abstraction nếu cần; implement trong
  Infrastructure.
- Dùng conditional update theo ID + concurrency token. Persist đúng field thay
  đổi hoặc aggregate state theo transaction boundary đã chọn.
- Phân biệt not-found và concurrency conflict; không biến stale update thành
  success.
- Nếu có outbox/event, bảo đảm nhất quán với transaction. Thêm migration chỉ khi
  schema đổi.
- Đăng ký adapter trong `DependencyInjection.cs`; API không gọi DbContext.

### 5. API endpoint

- Request parser/converter phải giữ được ba trạng thái absent/null/value.
- Dùng `MapPatch(ApiRoutes....)`, operation name/tag ổn định,
  `.Accepts<T>(<media-type>)` và `.Produces...` cho status thực tế.
- Parse route/header an toàn, map allowlist sang command, truyền
  `CancellationToken`; không đặt business logic trong endpoint.
- Success JSON dùng `ApiResponseFactory.Success(...)`; nếu contract dùng `204`,
  body phải rỗng. Đăng ký endpoint trong `Program.cs`.

### 6. Error envelope

- Expected error đi qua shared middleware/application exception và trả
  `error.code`, safe `error.message`, `error.details`, `meta.traceId`.
- Tối thiểu xem xét `400` malformed/empty/unknown/invalid field, `401`, `403`,
  `404`, `409` concurrency/business conflict, `415` unsupported patch media type
  và `5xx`.
- Không lộ stack trace, SQL, credentials, storage key hoặc implementation detail.

### 7. Coverage

- **Unit:** presence semantics cho từng mutable field, null policy, allowlist,
  cross-field validation, empty/no-op patch và concurrency.
- **Component:** Content-Type/binding/parser, route/header, success envelope hoặc
  `204`, error envelope, authorization và metadata.
- **Integration:** update đúng subset trên database thật, giữ nguyên field absent,
  clear field nullable, rollback và concurrent writers.
- Tests phải deterministic, độc lập; TestServer + stub không thay thế database
  integration coverage.

### 8. Postman và kiểm tra cuối

- Cập nhật `postman/MiniProjectKaopiz.postman_collection.json` với request PATCH,
  đúng media type, variables/auth/concurrency header và examples cho value,
  explicit null, absent field, disallowed field, success/error.
- Chạy test feature/project, integration test, `dotnet build`, toàn bộ
  `dotnet test`, `dotnet format --verify-no-changes`.
- Đối chiếu code, API doc, business-flow 1:1 và Postman trước bàn giao.

## Tiêu chí hoàn thành

- PATCH chỉ thay đổi allowed fields có mặt và phân biệt absent với explicit null.
- Concurrency, no-op, status và side effects nhất quán giữa code/tests/docs/Postman.
- Dependency giữ `Api -> Application <- Infrastructure`; không gộp method khác.
