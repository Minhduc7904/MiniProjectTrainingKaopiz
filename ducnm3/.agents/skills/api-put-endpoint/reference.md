# Tham chiếu triển khai PUT

## Nguồn quy tắc bắt buộc

- `AGENTS.md`: phạm vi chỉnh sửa, Git flow và thứ tự đọc tài liệu.
- `rules/documentation-language.md`: viết tiếng Việt, giữ nguyên technical terms,
  identifier và API contract.
- `rules/code-quality.md`: shared constants, module nhỏ và xử lý lỗi rõ ràng.
- `rules/testing.md`: coverage theo hành vi, deterministic và chạy quality checks.
- `rules/api-documentation.md`: nội dung bắt buộc của endpoint document.
- `docs/architecture/clean-architecture.md`: dependency direction và cấu trúc
  bốn layer.
- `docs/api/_templates/endpoint.md`: khung API document.
- `docs/api/shared/response-format.md` và
  `docs/api/shared/error-format.md`: success/error envelope chuẩn.
- `docs/business-flows/README.md`: một file cho một nghiệp vụ độc lập.

## Bản đồ project

Với `<Area>` là `Course`, `Student`, `Media`, `Notification` hoặc `Scheduler`,
và `<Service>` là tên service tương ứng:

```text
backend/Services/<Area>/
├── <Service>.Domain/
├── <Service>.Application/
│   ├── Abstractions/
│   ├── Features/<Resources>/Replace/
│   └── DependencyInjection.cs
├── <Service>.Infrastructure/
│   ├── Persistence/
│   └── DependencyInjection.cs
├── <Service>.Api/
│   ├── Contracts/Requests/
│   ├── Contracts/Responses/
│   ├── Endpoints/
│   ├── Mappers/
│   └── Program.cs
├── <Service>.UnitTests/
└── <Service>.IntegrationTests/  # tạo cạnh service khi cần
```

Shared API contract:

```text
backend/BuildingBlocks/BuildingBlocks.Contracts/Api/
├── ApiRoutes.cs
└── ApiConstants.cs

backend/BuildingBlocks/BuildingBlocks.Presentation/
├── Api/
├── Middleware/
└── Extensions/
```

Tài liệu và collection:

```text
docs/api/<owning-service>/endpoints/put-<resource>.md
docs/business-flows/<domain>/<put-resource>.md
postman/MiniProjectKaopiz.postman_collection.json
```

API doc thuộc owner service, không thuộc caller. Mỗi endpoint PUT chỉ có một API
doc và một business-flow file ánh xạ 1:1.

## Convention trong repository

- API dùng ASP.NET Core Minimal APIs và đăng ký qua extension `Map...`.
- Route lấy từ `BuildingBlocks.Contracts.Api.ApiRoutes`; route public qua Gateway
  dùng helper/prefix hiện có.
- Endpoint dùng `.WithName(...)`, `.WithTags(ServiceNames.<Service>)`,
  `.Accepts<T>()` và `.Produces<T>()`.
- JSON success dùng `ApiResponse<T>` và `ApiResponseFactory.Success(...)`.
- JSON error dùng `ApiErrorResponse`, được shared middleware map từ expected
  application exception.
- Business error code nằm trong Application, ví dụ `<Service>ErrorCodes`; shared
  transport/dependency code nằm trong `ApiErrorCodes`.
- Application feature thường có command/query, handler, result và abstraction
  persistence. Infrastructure implement abstraction và đăng ký qua
  `DependencyInjection.cs`.
- Public type trong feature nên ở file riêng. Endpoint chỉ parse/map/delegate.
- Test framework hiện tại là NUnit. Component-style endpoint test dùng
  `WebApplication`, `UseTestServer()`, `UseSharedApiMiddleware()` và HTTP client.
- Integration test project đặt cạnh service và dùng dependency/container thật,
  biệt lập.

## Quyết định riêng của PUT

### Full replacement

- Request liệt kê đầy đủ mọi client-managed field của representation.
- Field vắng mặt không có nghĩa là giữ nguyên. Required field vắng mặt là lỗi;
  optional field vắng mặt nhận default/null theo contract thay thế.
- Không cho client ghi server-managed fields như ID, audit timestamps hoặc
  computed values.
- Collection trong request thay thế toàn bộ collection thuộc phạm vi contract;
  không merge ngầm.

### Idempotency

Hai request giống nhau tới cùng URI và cùng precondition phải tạo cùng trạng
thái cuối. Handler không tăng counter, tạo record phụ hoặc phát event lặp lại khi
không có state transition. Nếu event bắt buộc, dùng state-change detection,
idempotency record hoặc outbox phù hợp.

### Concurrency

Ưu tiên optimistic concurrency:

- Header `If-Match` mang ETag/version; hoặc field `version` nếu contract hiện có
  đã chuẩn hóa cách này.
- Nêu rõ yêu cầu token trong API doc và Postman.
- Adapter update theo điều kiện `WHERE id = ... AND version = ...`.
- `affectedRows == 0` phải được phân loại thành `404` hay concurrency conflict
  bằng truy vấn/chiến lược an toàn, không mặc định coi mọi trường hợp là not-found.
- Khi thành công, tăng version và trả ETag/version mới nếu response contract cần.

### Success

- Dùng `200 OK` khi caller cần representation/version mới; response JSON phải có
  envelope và `meta.traceId`.
- Dùng `204 No Content` khi không cần representation; body phải rỗng.
- Không trả đồng thời cả hai tùy ý. Chọn một hành vi và khóa bằng docs/tests.

## Lệnh kiểm tra

Chạy từ repository root và thay placeholder bằng project thực:

```bash
dotnet test backend/Services/<Area>/<Service>.UnitTests/<Service>.UnitTests.csproj
dotnet test backend/Services/<Area>/<Service>.IntegrationTests/<Service>.IntegrationTests.csproj
dotnet build
dotnet test
dotnet format --verify-no-changes
```

Nếu service chưa có integration project, tạo theo convention khi hành vi cần
database boundary; không tuyên bố integration coverage bằng TestServer với
repository stub.
