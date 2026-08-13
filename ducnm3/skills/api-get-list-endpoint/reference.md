# Tham chiếu GET list endpoint

## Nguồn bắt buộc

- `AGENTS.md`
- `rules/workflow-skills.md`
- `rules/documentation-language.md`
- `rules/code-quality.md`
- `rules/testing.md`
- `rules/api-documentation.md`
- `docs/architecture/clean-architecture.md`
- `docs/api/README.md`
- `docs/api/_templates/endpoint.md`
- `docs/api/shared/response-format.md`
- `docs/api/shared/error-format.md`
- API, business-flow, database/index và performance docs liên quan
- `skills/api-get-list-endpoint/template.md`
- Các test skill; migration skill nếu schema/index thay đổi

## Bản đồ project

```text
backend/
├── BuildingBlocks/
│   ├── BuildingBlocks.Contracts/Api/
│   │   ├── ApiRoutes.cs
│   │   ├── ApiConstants.cs
│   │   └── ApiContracts.cs
│   └── BuildingBlocks.Presentation/Api/ApiResponseFactory.cs
└── Services/<Service>/
    ├── <ServiceName>.Domain/
    ├── <ServiceName>.Application/
    │   ├── Features/<Resources>/GetList/
    │   ├── Abstractions/
    │   └── DependencyInjection.cs
    ├── <ServiceName>.Infrastructure/
    │   ├── Persistence/
    │   └── DependencyInjection.cs
    ├── <ServiceName>.Api/
    │   ├── Contracts/
    │   ├── Endpoints/
    │   ├── Mappers/
    │   └── Program.cs
    ├── <ServiceName>.UnitTests/
    └── <ServiceName>.IntegrationTests/

docs/api/<owning-service>/endpoints/get-<resources>.md
docs/business-flows/<domain>/get-<resources>.md
docs/database/
docs/tests/
postman/MiniProjectKaopiz.postman_collection.json
```

## Nguồn mẫu trong repository

- Shared envelope và pagination contract mong đợi:
  `docs/api/shared/response-format.md`.
- Cursor contract:
  `docs/api/course-service/endpoints/get-courses-cursor.md`.
- Offset contract:
  `docs/api/student-service/endpoints/get-students.md`.
- EF read projection mẫu:
  `backend/Services/Student/StudentService.Infrastructure/Persistence/EfStudentRepository.cs`.
- TestServer convention:
  `backend/Services/Student/StudentService.UnitTests/StudentEndpointTests.cs`.
- Shared route:
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`.

Lưu ý: `ApiContracts.cs` hiện có `ResponseMeta(string TraceId)` trong
implementation được kiểm tra tại thời điểm viết skill, trong khi
`docs/api/shared/response-format.md` yêu cầu `meta.pagination` cho list. Khi làm
endpoint list thật, phải đồng bộ shared contract và `ApiResponseFactory` thay vì
tạo wrapper riêng hoặc bỏ pagination metadata.

## Contract filter

Với từng filter, ghi rõ:

- query name và kiểu;
- optional/required, default và min/max;
- trim/case/Unicode/time-zone normalization;
- exact/prefix/range/multi-value semantics;
- cách kết hợp nhiều filter (`AND` hay nhóm `OR`);
- cách xử lý empty string, repeated key và unknown value;
- visibility/tenant filter luôn do server áp dụng.

Không map trực tiếp query string thành raw SQL. Không cho client chọn column
ngoài allowlist.

## Contract sort và total order

Mọi page phải dựa trên total order xác định:

```text
ORDER BY <allowed-key> <direction>, id <direction-or-fixed>
```

- Tie-breaker phải unique và không thay đổi trong vòng đời item, thường là `id`.
- Nếu sort key nullable, chốt null ordering.
- Nếu sort theo text, chốt collation/case semantics.
- Cursor predicate và `ORDER BY` phải cùng key, direction và null semantics.
- Sort mặc định phải được ghi trong API doc và Postman.
- Invalid sort key/direction trả validation error, không fallback âm thầm.

## Chọn pagination

Chọn đúng một contract cho mỗi endpoint.

### Cursor/keyset

Phù hợp feed lớn hoặc dữ liệu thay đổi thường xuyên:

- Query: `limit`, `cursor` optional.
- Metadata: `type=cursor`, `limit`, `nextCursor`, `hasNextPage`.
- Cursor opaque, có version; nên ký hoặc validate integrity.
- Decode thất bại/version không hỗ trợ/sort mismatch trả `400 INVALID_CURSOR`.
- Query lấy `limit + 1`; bỏ item dư và chỉ tạo next cursor khi còn trang.
- Không yêu cầu `totalItems`; không encode raw ID thành token dễ đọc.
- Ghi semantics khi insert/delete/update sort key giữa hai request.

### Offset

Phù hợp UI cần nhảy trang và tổng số:

- Query: `page >= 1`, `pageSize` trong giới hạn.
- Metadata: `type=offset`, `page`, `pageSize`, `totalItems`, `totalPages`.
- `totalPages = totalItems == 0 ? 0 : ceiling(totalItems / pageSize)`.
- `Skip((page - 1) * pageSize).Take(pageSize)` với overflow-safe math.
- Count và page query dùng cùng filter/visibility.
- Ghi rõ dữ liệu có thể dịch chuyển giữa các page khi concurrent writes.

Không trả cursor fields trong offset response hoặc totals trong cursor response
trừ khi shared contract đã quy định rõ.

## Query và index

Thứ tự triển khai query:

1. Tenant/authorization/soft-delete visibility.
2. Filter allowlist.
3. Stable ordering.
4. Keyset predicate hoặc offset.
5. `Take`.
6. Projection.

Sử dụng `AsNoTracking()`. Tránh `Include` collection nếu chỉ cần projection.
Không materialize trước filter/page. Đánh giá composite index bắt đầu bằng
equality filter phổ biến rồi đến sort/tie-breaker; xác nhận bằng query plan khi
volume lớn. Schema/index mới phải theo migration workflow và cập nhật database
docs.

## Shared response envelope

Cursor:

```json
{
  "data": [],
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "cursor",
      "limit": 20,
      "nextCursor": null,
      "hasNextPage": false
    }
  }
}
```

Offset:

```json
{
  "data": [],
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "offset",
      "page": 1,
      "pageSize": 20,
      "totalItems": 0,
      "totalPages": 0
    }
  }
}
```

List rỗng là `200`, không phải `404`.

## Tài liệu 1:1

API doc duy nhất nêu:

- method/direct/public path, owner, auth/visibility;
- từng filter và ví dụ kết hợp;
- sort keys, direction, default, tie-breaker/null order;
- cursor hoặc offset query/schema/example;
- empty result;
- validation/auth/dependency errors;
- safe/idempotent/read-only, cache và consistency semantics.

Business-flow file duy nhất chỉ mô tả list/search đó:

- actor và mục tiêu tìm kiếm;
- filter/visibility;
- stable ordering;
- page đầu/page tiếp hoặc nhảy offset;
- empty/invalid cases;
- dữ liệu chỉ đọc và query dependencies.

Không gộp GET detail hoặc export vào flow.

## Postman

- Method `GET`, không `body`.
- Variables cho base URL, filter, sort và page size.
- Success script kiểm tra `data` là array, `meta.traceId` và pagination fields.
- Cursor: lưu `nextCursor`, gọi trang tiếp, kiểm tra không trùng ID.
- Offset: kiểm tra page/pageSize/totals, page đầu/cuối/rỗng.
- Cases invalid filter/sort/cursor/page/pageSize và auth.
- Không đưa secret hoặc opaque cursor hard-code lâu dài vào collection.

## Coverage checklist

### Unit

- [ ] Default và boundary của filter/page size.
- [ ] Unknown/invalid filter và sort.
- [ ] Stable tie-breaker, null ordering.
- [ ] Cursor encode/decode/version/integrity hoặc offset math.
- [ ] Empty result metadata.
- [ ] Cancellation propagation.

### Component

- [ ] Query binding thật, GET không body.
- [ ] `200` standard envelope.
- [ ] Pagination metadata đúng loại.
- [ ] Validation error details an toàn.
- [ ] Auth/visibility và cache header.
- [ ] Endpoint metadata/OpenAPI đúng.

### Integration

- [ ] Filter combinations và normalization.
- [ ] Mọi allowed sort với duplicate values.
- [ ] First/middle/last/empty page.
- [ ] Concurrent insert/delete/update semantics.
- [ ] Không duplicate/skip ngoài semantics đã chốt.
- [ ] Count và page dùng cùng predicate.
- [ ] Index/query plan cho volume mục tiêu.

## Lệnh kiểm tra

```bash
dotnet test backend/Services/<Service>/<ServiceName>.UnitTests/<ServiceName>.UnitTests.csproj
dotnet test backend/Services/<Service>/<ServiceName>.IntegrationTests/<ServiceName>.IntegrationTests.csproj
dotnet build backend/Lms.sln -m:1
dotnet test backend/Lms.sln -m:1
dotnet format backend/Lms.sln --verify-no-changes
```
