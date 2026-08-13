---
name: api-put-endpoint
description: Tạo hoặc thay đổi một endpoint PUT ASP.NET Core theo Clean Architecture, bao gồm contract, triển khai, tài liệu và kiểm thử.
---

# API PUT endpoint

## Khi sử dụng

Áp dụng riêng cho một endpoint `PUT` thay thế đầy đủ tài nguyên. Không dùng quy
trình này cho `PATCH`, `DELETE` hoặc gộp nhiều HTTP method vào cùng một thay đổi.

## Tài liệu bắt buộc

Trước khi sửa code:

1. Đọc toàn bộ [`reference.md`](reference.md) để xác định đúng project, convention
   và nguồn contract dùng chung.
2. Đọc toàn bộ [`template.md`](template.md) và dùng các checklist/code skeleton
   phù hợp.
3. Đọc `rules/documentation-language.md`, `rules/code-quality.md`,
   `rules/testing.md`, `rules/api-documentation.md` và
   `rules/workflow-skills.md`.
4. Đọc tài liệu API, business flow, database và architecture liên quan cùng
   implementation hiện có.
5. Đọc riêng `skills/test-unit/`, `skills/test-component/` và
   `skills/test-integration/`; đọc `skills/database-migration/` nếu đổi schema
   hoặc index.

Không bắt đầu triển khai nếu chưa chốt ý nghĩa thay thế đầy đủ, status thành
công, validation và cơ chế concurrency.

## Quy trình

### 1. Chốt contract và tài liệu trước

- Xác định owner service, `PUT /api/<resources>/{id}`, authentication,
  authorization/ownership và toàn bộ request fields đại diện cho trạng thái có
  thể thay thế.
- Không diễn giải field vắng mặt như “giữ nguyên”; field bắt buộc bị thiếu phải
  trả validation error. Nêu rõ field server quản lý không nhận từ client.
- Chọn đúng một contract thành công:
  - `200 OK` với response envelope khi trả representation sau cập nhật; hoặc
  - `204 No Content` khi không trả body.
- Ghi rõ idempotency: cùng request và cùng version gửi lại không tạo thêm side
  effect hoặc thay đổi trạng thái ngoài dự kiến.
- Chọn concurrency token, thường là `If-Match`/ETag hoặc `version` trong request;
  quy định token thiếu, sai định dạng và stale token.
- Tạo/cập nhật đúng **một** file API tại
  `docs/api/<owning-service>/endpoints/put-<resource>.md`.
- Tạo/cập nhật đúng **một** file business flow ánh xạ 1:1 tại
  `docs/business-flows/<domain>/<put-resource>.md`. File này chỉ mô tả nghiệp vụ
  của endpoint PUT đang làm.

### 2. Route và constants dùng chung

- Thêm route template/path builder vào
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`; không viết
  magic path tại endpoint, client hoặc test.
- Tái sử dụng `ApiErrorCodes`, header constants và status constants dùng chung.
- Đặt business error code trong Application của owner service; không sao chép
  string giữa các layer.
- Thêm test cho route/path builder khi thay đổi shared route contract.

### 3. Application feature

- Tạo feature riêng, ví dụ
  `<Service>.Application/Features/<Resources>/Replace/`.
- Tách `Replace<Resource>Command`, result (nếu trả `200`), validator, handler và
  public type thành file riêng theo convention hiện có.
- Validate ID, toàn bộ required fields, độ dài/range/enum, quan hệ giữa fields và
  concurrency token trước khi ghi.
- Handler tải aggregate hiện tại, kiểm tra not-found/authorization/business
  preconditions/concurrency, áp dụng replacement và lưu một transaction.
- Định nghĩa rõ side effect. Chỉ phát event hoặc gọi dependency sau khi trạng
  thái hợp lệ; bảo đảm retry cùng input không nhân đôi side effect.
- Application chỉ phụ thuộc Domain và abstraction; không phụ thuộc ASP.NET Core,
  EF Core, MySQL hoặc project Api.

### 4. Infrastructure adapter

- Mở rộng repository/unit-of-work abstraction tại Application nếu cần.
- Implement adapter trong `<Service>.Infrastructure`; thực hiện update có điều
  kiện theo ID và concurrency token, phân biệt `not found` với `conflict`.
- Giữ transaction boundary nhất quán. Nếu có event/outbox, ghi cùng transaction
  hoặc nêu rõ chiến lược nhất quán.
- Thêm migration chỉ khi schema thay đổi và tuân theo skill migration riêng.
- Đăng ký adapter trong `DependencyInjection.cs`; không để API gọi DbContext trực
  tiếp.

### 5. API endpoint

- Tạo request/response contract và mapper trong project Api; map request sang
  command rõ ràng.
- Dùng `MapPut(ApiRoutes....)` với tên operation ổn định, tag đúng service,
  `.Accepts<T>("application/json")` và `.Produces...` cho mọi status thực tế.
- Parse route/header an toàn, truyền `CancellationToken`, không chứa business
  logic trong endpoint.
- Với `200`, trả `ApiResponseFactory.Success(result, traceId)`. Với `204`, trả
  `Results.NoContent()` và không gửi JSON body.
- Đăng ký endpoint trong `Program.cs`.

### 6. Error envelope

- Expected error phải đi qua shared middleware/application exception và có dạng
  `error.code`, safe `error.message`, `error.details`, `meta.traceId`.
- Tối thiểu xem xét `400` validation, `401`, `403`, `404`, concurrency conflict
  (`409` hoặc status đã được contract dự án chốt) và `5xx`.
- Không để lộ stack trace, SQL, credentials, internal storage key hoặc chi tiết
  adapter.

### 7. Coverage

- **Unit:** validator, replacement đầy đủ, not-found, business rules,
  idempotency và stale/current concurrency token.
- **Component:** TestServer gọi route thật, binding/header, `200` hoặc `204`,
  standard error envelope, authorization và endpoint metadata.
- **Integration:** database/container thật cho conditional update, transaction,
  rollback, persistence của mọi field và hai writer cạnh tranh.
- Mỗi test deterministic, độc lập; không thay component/integration test bằng
  mock-only unit test.

### 8. Postman và kiểm tra cuối

- Cập nhật `postman/MiniProjectKaopiz.postman_collection.json` với một request PUT
  đúng service, variables, headers, full payload, success test và các case
  validation/not-found/concurrency.
- Chạy theo thứ tự: test feature/project liên quan, integration test liên quan,
  `dotnet build`, toàn bộ `dotnet test`, `dotnet format --verify-no-changes`.
- Sửa mọi lỗi do thay đổi gây ra. Đối chiếu implementation với API doc,
  business-flow 1:1 và Postman trước khi bàn giao.

## Tiêu chí hoàn thành

- PUT thực sự là full replacement và idempotent.
- Contract `200` hoặc `204`, validation và concurrency nhất quán giữa code,
  tests, API doc, business flow và Postman.
- Dependency vẫn đi `Api -> Application <- Infrastructure`, Application phụ
  thuộc Domain; không có method khác bị gộp vào skill này.
