---
name: api-get-list-endpoint
description: Tạo hoặc thay đổi một endpoint GET list hoặc search ASP.NET Core theo Clean Architecture, gồm filter, sort, pagination, tài liệu và kiểm thử.
---

# API GET list endpoint

## Khi sử dụng

Áp dụng riêng cho một endpoint `GET` trả collection, list hoặc search. Không dùng
cho GET detail hay method khác; không gộp nhiều HTTP method trong cùng skill.

## Tài liệu bắt buộc

Trước khi sửa code:

1. Đọc toàn bộ [`reference.md`](reference.md) để xác định path, pagination
   envelope, query convention và ranh giới layer.
2. Đọc toàn bộ [`template.md`](template.md), rồi dùng checklist/code skeleton
   tương ứng với **cursor hoặc offset**, không trộn hai kiểu ngầm định.
3. Đọc `rules/documentation-language.md`, `rules/code-quality.md`,
   `rules/testing.md`, `rules/api-documentation.md` và
   `rules/workflow-skills.md`.
4. Đọc API doc, business flow, database/index, architecture và implementation
   liên quan.
5. Đọc riêng `.agents/skills/test-unit/`, `.agents/skills/test-component/` và
   `.agents/skills/test-integration/`; đọc migration skill nếu đổi schema/index.

Không triển khai khi chưa chốt filter allowlist, sort allowlist, stable total
order và đúng một pagination contract.

## Quy trình

### 1. Chốt contract và tài liệu trước

- Xác định `GET /api/<resources>`, owner service, auth, visibility và response
  item fields. GET không có request body, phải safe và idempotent.
- Liệt kê từng filter: tên, kiểu, mặc định, normalization, kết hợp `AND`/`OR`,
  validation và semantics của giá trị rỗng.
- Liệt kê sort keys/direction được phép. Mọi sort phải có tie-breaker duy nhất,
  thường là `id`, để tạo total order ổn định.
- Chọn đúng một kiểu:
  - **cursor**: `limit`, opaque `cursor`, `nextCursor`, `hasNextPage`; hoặc
  - **offset**: `page`, `pageSize`, `totalItems`, `totalPages`.
- Chốt invalid cursor/page/sort/filter error và cache policy phù hợp.
- Tạo/cập nhật đúng **một** API doc tại
  `docs/api/<owning-service>/endpoints/get-<resources>.md`.
- Tạo/cập nhật đúng **một** business-flow file 1:1 tại
  `docs/business-flows/<domain>/get-<resources>.md`.

### 2. Route và constants dùng chung

- Thêm route/path builder vào
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`.
- Tái sử dụng shared route/header/error/status constants; business filter/sort
  values thuộc Application của owner service.
- Nếu shared `ResponseMeta` chưa biểu diễn pagination contract đã chọn, mở rộng
  shared contract/factory có chủ đích và thêm compatibility tests; không tạo
  endpoint-specific wrapper trái `docs/api/shared/response-format.md`.
- Test route và shared pagination contract khi thay đổi.

### 3. Application feature

- Tạo `<Service>.Application/Features/<Resources>/GetList/` hoặc tên tương đương.
- Tách query/filter/sort/page request, item/result, validator, handler,
  repository abstraction và cursor codec abstraction thành file riêng.
- Normalize và validate filter/sort/pagination trước khi gọi repository.
- Cursor phải được decode/validate như opaque token; không tin field do client
  gửi và không làm lộ internal database ID/sort value.
- Handler chỉ điều phối read use case; Application không phụ thuộc ASP.NET Core,
  EF Core, MySQL hoặc project Api.

### 4. Infrastructure adapter

- Dùng `AsNoTracking()`, apply visibility/filter, stable `OrderBy` + `ThenBy`
  tie-breaker rồi mới paginate và project.
- Cursor: dùng keyset predicate khớp hoàn toàn sort direction, lấy `limit + 1`,
  tạo `hasNextPage` và next cursor từ item cuối được trả.
- Offset: validate page/pageSize, áp dụng cùng filter cho `CountAsync`, sau đó
  `Skip`/`Take`; nêu rõ consistency semantics giữa count và page query.
- Tránh N+1, client-side evaluation, unbounded query và dynamic sort không
  allowlist. Thêm/đánh giá index theo filter + sort.
- Đăng ký adapter; không truy vấn chéo database.

### 5. API endpoint

- Dùng `MapGet(ApiRoutes....)` và bind query parameters, không bind request body.
- Map HTTP query sang Application query, truyền `CancellationToken`.
- Trả standard envelope với đúng `meta.pagination` cursor hoặc offset.
- Khai báo operation name, tag, `.Produces...` cho `200` và mọi error thực tế;
  đăng ký endpoint trong `Program.cs`.
- Thiết lập cache header rõ ràng; list theo quyền hoặc dữ liệu biến động thường
  dùng `no-store` nếu chưa có invalidation policy.

### 6. Error envelope

- Validation của filter/sort/cursor/page đi qua shared error envelope:
  `error.code`, safe `error.message`, `error.details`, `meta.traceId`.
- Không trả `404` chỉ vì page/list rỗng; trả `200` với `data: []` và pagination
  metadata hợp lệ.
- Không để raw cursor parsing exception, SQL hoặc internal sort key lộ ra.

### 7. Coverage

- **Unit:** normalization/validation, filter combinations, sort allowlist,
  stable tie-breaker, cursor encode/decode hoặc offset math, empty result.
- **Component:** TestServer với query thật; `200` envelope, pagination metadata,
  invalid query error, auth/cache và không có body.
- **Integration:** database/container thật; filter/sort, boundary pages,
  duplicate sort values, insert/delete giữa pages, no duplicate/skip theo
  semantics đã chốt, count và index/query behavior.
- Test deterministic, dữ liệu cô lập; không thay component/integration bằng
  mock-only unit test.

### 8. Postman và kiểm tra cuối

- Cập nhật `postman/MiniProjectKaopiz.postman_collection.json` với request GET
  list trong đúng service, variables, filter/sort, pagination, success tests,
  empty result và invalid query cases.
- Với cursor, lưu `nextCursor` và gọi trang sau. Với offset, test page đầu/cuối
  và metadata totals.
- Chạy test feature/project, integration test, rồi
  `dotnet build backend/Lms.sln -m:1`,
  `dotnet test backend/Lms.sln -m:1`,
  `dotnet format backend/Lms.sln --verify-no-changes`.
- Đối chiếu implementation, đúng một API doc, business flow 1:1 và Postman.

## Tiêu chí hoàn thành

- Endpoint chỉ dùng GET list, safe, idempotent, không body và không side effect.
- Filter/sort allowlist, total order ổn định và đúng một cursor/offset contract
  nhất quán giữa code, tests, docs và Postman.
- Dependency vẫn đi `Api -> Application <- Infrastructure`; Application phụ
  thuộc Domain và abstraction.
