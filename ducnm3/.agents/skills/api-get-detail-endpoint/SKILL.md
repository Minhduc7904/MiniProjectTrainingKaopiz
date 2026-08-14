---
name: api-get-detail-endpoint
description: Tạo hoặc thay đổi một endpoint GET chi tiết ASP.NET Core theo Clean Architecture, bao gồm contract, cache, tài liệu và kiểm thử.
---

# API GET detail endpoint

## Khi sử dụng

Áp dụng riêng cho một endpoint `GET` trả một tài nguyên theo ID. Không dùng quy
trình này cho list/search, `POST`, `PUT`, `PATCH` hoặc `DELETE`; không gộp nhiều
HTTP method trong cùng skill.

## Tài liệu bắt buộc

Trước khi sửa code:

1. Đọc toàn bộ [`reference.md`](reference.md) để xác định đúng project, path,
   convention, response envelope và cache contract.
2. Đọc toàn bộ [`template.md`](template.md), rồi dùng checklist và code skeleton
   phù hợp với feature.
3. Đọc `rules/documentation-language.md`, `rules/code-quality.md`,
   `rules/testing.md`, `rules/api-documentation.md` và
   `rules/workflow-skills.md`.
4. Đọc tài liệu API, business flow, database, architecture liên quan và
   implementation hiện có.
5. Đọc riêng `.agents/skills/test-unit/`, `.agents/skills/test-component/` và
   `.agents/skills/test-integration/` khi các loại test đó được yêu cầu. Nếu đổi schema,
   đọc thêm `.agents/skills/database-migration/`.

Không bắt đầu triển khai khi chưa chốt owner service, quyền đọc, representation,
`200`/`404` và cache policy.

## Quy trình

### 1. Chốt contract và tài liệu trước

- Xác định `GET /api/<resources>/{id}`, public path qua Gateway,
  authentication, authorization/ownership và response fields.
- GET phải **safe**, **idempotent**, không có request body và không tạo side
  effect. Chỉ đọc dependency cần thiết.
- Chốt `200 OK` khi tài nguyên tồn tại, `404 <RESOURCE>_NOT_FOUND` khi không tồn
  tại, `400 VALIDATION_FAILED` cho ID sai và các lỗi auth/dependency thực tế.
- Chọn cache policy rõ ràng: `no-store`, `private/public max-age`, hoặc
  ETag/`If-None-Match` với `304 Not Modified`. Không để cache behavior ngầm định.
- Tạo/cập nhật đúng **một** API doc tại
  `docs/api/<owning-service>/endpoints/get-<resource>-by-id.md`.
- Tạo/cập nhật đúng **một** business-flow file ánh xạ 1:1 tại
  `docs/business-flows/<domain>/get-<resource>-by-id.md`; file chỉ mô tả GET này.

### 2. Route và constants dùng chung

- Thêm route template, service path và public path builder vào
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`.
- Tái sử dụng `ApiErrorCodes`, `ApiHeaderNames`, `ServiceNames` và
  `GatewayRoutePrefixes`; thêm shared cache/header constant nếu nhiều component
  cần dùng, không viết magic string.
- Đặt business error code/message trong Application của owner service.
- Thêm test route/path builder khi shared route contract thay đổi.

### 3. Application feature

- Tạo feature riêng, ví dụ
  `<Service>.Application/Features/<Resources>/GetById/`.
- Tách query, result/DTO, handler, repository abstraction, error code/message và
  public type thành file riêng theo convention của service.
- Validate ID và input liên quan; kiểm tra quyền, visibility và business
  precondition trong Application.
- Handler gọi abstraction read-only, trả projection cần thiết hoặc ném
  application exception `404`; truyền `CancellationToken`.
- Application chỉ phụ thuộc Domain và abstraction, không phụ thuộc ASP.NET Core,
  EF Core, MySQL hoặc project Api.

### 4. Infrastructure adapter

- Implement query adapter trong `<Service>.Infrastructure`; ưu tiên
  `AsNoTracking()`, filter theo ID/visibility và `Select` trực tiếp sang
  projection để tránh N+1/over-fetch.
- Không truy vấn chéo database; dependency service phải đi qua typed QUERY
  client ở Infrastructure.
- Đăng ký adapter trong `DependencyInjection.cs`.
- Không thêm transaction cho read-only GET. Chỉ thêm migration nếu contract cần
  schema/index mới và đã theo workflow migration.

### 5. API endpoint

- Dùng `MapGet(ApiRoutes....)`; parse route an toàn, truyền `CancellationToken`,
  map result sang API response và không đặt business logic tại endpoint.
- Trả `ApiResponseFactory.Success(data, context.TraceIdentifier)` cho `200`.
- Thiết lập cache header đúng contract. Nếu dùng ETag, xử lý
  `If-None-Match`/`304` nhất quán và không gửi body cho `304`.
- Khai báo operation name, tag và `.Produces...` cho mọi status thực tế.
- Đăng ký endpoint trong `Program.cs`; không thêm body binding cho GET.

### 6. Error envelope

- Expected error đi qua shared middleware/application exception với
  `error.code`, safe `error.message`, `error.details`, `meta.traceId`.
- Tối thiểu kiểm tra `400`, `401`, `403`, `404` và `5xx` theo contract thực tế.
- Không trả `200` với `data: null` thay cho `404`.
- Không làm lộ stack trace, SQL, credentials, object-storage key hoặc chi tiết
  adapter.

### 7. Coverage

- **Unit:** ID validation, found/not-found, authorization/visibility, mapping và
  không phát sinh write/side effect.
- **Component:** TestServer gọi route thật; kiểm tra không có body request,
  `200`, `404`, error envelope, cache header, auth và `304` nếu dùng ETag.
- **Integration:** database/container thật; kiểm tra projection, visibility,
  not-found, read-only behavior, query/index quan trọng và dependency adapter nếu
  có.
- Test phải deterministic, độc lập và truyền cancellation; không thay
  component/integration coverage bằng mock-only test.

### 8. Postman và kiểm tra cuối

- Cập nhật `postman/MiniProjectKaopiz.postman_collection.json` trong đúng service
  với request GET không body, variables, header cần thiết, success test, `404`
  test và cache/conditional request test phù hợp.
- Chạy test feature/project, integration test liên quan, sau đó:
  `dotnet build backend/Lms.sln -m:1`,
  `dotnet test backend/Lms.sln -m:1`,
  `dotnet format backend/Lms.sln --verify-no-changes`.
- Sửa lỗi do thay đổi gây ra và đối chiếu code, một API doc, business flow 1:1
  và Postman trước khi bàn giao.

## Tiêu chí hoàn thành

- Endpoint chỉ dùng GET, safe, không body, idempotent và không có side effect.
- `200`/`404`, quyền đọc, response/error envelope và cache policy nhất quán giữa
  implementation, tests, API doc, business flow và Postman.
- Dependency vẫn đi `Api -> Application <- Infrastructure`; Application phụ
  thuộc Domain và abstraction.
