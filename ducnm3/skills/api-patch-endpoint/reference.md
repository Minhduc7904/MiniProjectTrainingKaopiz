# Tham chiếu triển khai PATCH

## Nguồn quy tắc bắt buộc

- `AGENTS.md`: phạm vi, Git flow và required reading.
- `rules/documentation-language.md`: tiếng Việt tự nhiên, giữ technical terms,
  identifier và contract.
- `rules/code-quality.md`, `rules/testing.md`,
  `rules/api-documentation.md`.
- `docs/architecture/clean-architecture.md`: bốn layer và dependency direction.
- `docs/api/_templates/endpoint.md`.
- `docs/api/shared/response-format.md` và
  `docs/api/shared/error-format.md`.
- `docs/business-flows/README.md`.
- Tham khảo contract hiện có tại
  `docs/api/notification-service/endpoints/patch-notification-read.md`, nhưng
  không sao chép hành vi nếu resource mới có contract khác.

## Bản đồ project

```text
backend/Services/<Area>/
├── <Service>.Domain/
├── <Service>.Application/
│   ├── Abstractions/
│   ├── Features/<Resources>/Patch/
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
└── <Service>.IntegrationTests/

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
docs/api/<owning-service>/endpoints/patch-<resource>.md
docs/business-flows/<domain>/<patch-resource>.md
postman/MiniProjectKaopiz.postman_collection.json
```

API doc luôn nằm dưới owner service. Mỗi PATCH có đúng một endpoint doc và một
business-flow file ánh xạ 1:1.

## Convention trong repository

- ASP.NET Core Minimal APIs, endpoint extension `Map...`, đăng ký trong
  `<Service>.Api/Program.cs`.
- Route dùng `BuildingBlocks.Contracts.Api.ApiRoutes`, operation name qua
  `.WithName`, tag qua `ServiceNames`.
- Endpoint khai báo `.Accepts<T>()`, `.Produces<ApiResponse<T>>()` hoặc `204`, và
  `.Produces<ApiErrorResponse>()` cho expected status.
- Success JSON dùng `ApiResponseFactory.Success`; expected error được shared
  middleware map sang safe error envelope.
- Shared error code nằm trong `ApiErrorCodes`; business error code thuộc
  `<Service>.Application`.
- Application định nghĩa feature và persistence abstraction; Infrastructure
  implement adapter. Api parse JSON-specific representation và map sang command.
- NUnit được dùng cho unit/component/integration test. Component test có thể dùng
  `WebApplication`, `UseTestServer()` và `UseSharedApiMiddleware()`.

## Quyết định riêng của PATCH

### Chọn patch format

Chọn một format, document `Content-Type` và không trộn semantics:

1. **JSON Merge Patch** (`application/merge-patch+json`): field absent giữ nguyên;
   field `null` thường yêu cầu clear/remove. Chỉ dùng khi null semantics phù hợp.
2. **JSON Patch** (`application/json-patch+json`): danh sách operation/path; cần
   allowlist operation và JSON Pointer path, reject path lạ/read-only.
3. **Typed partial request** (`application/json`): cần presence wrapper hoặc
   custom `JsonConverter` để phân biệt absent/null/value.

`string? Name` đơn thuần không đủ: cả field absent và `"name": null` thường bind
thành `null`.

### Allowed fields

- Duy trì allowlist ở parser/mapper và policy Application phù hợp.
- Reject ID, audit field, computed field, concurrency field sai vị trí và unknown
  path/property.
- Không dùng reflection mass assignment từ request vào entity.
- Cross-field rule phải evaluate state cuối gồm current values + patch values.

### Absent, null và no-op

| Input state | Hành vi |
| --- | --- |
| Field absent | Giữ nguyên current value |
| Field present + value | Validate rồi thay field đó |
| Field present + null, nullable | Clear field |
| Field present + null, non-nullable | Validation error |

Patch `{}` hoặc patch tạo state giống hệt hiện tại phải có contract rõ ràng:
reject, hoặc success no-op. Nếu success no-op, không tăng version/`updated_at`
hay phát event trừ khi docs nói khác.

### Concurrency

- Ưu tiên `If-Match`/ETag hoặc version contract đang chuẩn hóa.
- Conditional update phải chặn lost update.
- Khi `affectedRows == 0`, phân biệt resource bị xóa với stale token.
- Trả version/ETag mới theo success contract nếu caller cần patch tiếp.

## Error mapping gợi ý

- `400 VALIDATION_ERROR`: malformed patch, empty patch bị cấm, unknown/disallowed
  field, invalid null/value/operation/path.
- `401 UNAUTHENTICATED`, `403 <RESOURCE_ACCESS_DENIED>`.
- `404 <RESOURCE_NOT_FOUND>`.
- `409 <RESOURCE_CONCURRENCY_CONFLICT>` hoặc business conflict.
- `415 UNSUPPORTED_MEDIA_TYPE` khi Content-Type không đúng contract.

Chỉ liệt kê status endpoint thực sự trả và dùng shared envelope cho JSON error.

## Lệnh kiểm tra

```bash
dotnet test backend/Services/<Area>/<Service>.UnitTests/<Service>.UnitTests.csproj
dotnet test backend/Services/<Area>/<Service>.IntegrationTests/<Service>.IntegrationTests.csproj
dotnet build
dotnet test
dotnet format --verify-no-changes
```

Integration coverage phải dùng persistence/dependency thật, biệt lập. TestServer
với repository stub là component test, không phải database integration test.
