# Tham chiếu triển khai DELETE

## Nguồn quy tắc bắt buộc

- `AGENTS.md`: phạm vi, Git flow và required reading.
- `rules/documentation-language.md`: tiếng Việt tự nhiên, giữ technical terms,
  identifier và contract.
- `rules/code-quality.md`, `rules/testing.md`,
  `rules/api-documentation.md`.
- `docs/architecture/clean-architecture.md`.
- `docs/api/_templates/endpoint.md`.
- `docs/api/shared/response-format.md` và
  `docs/api/shared/error-format.md`.
- `docs/business-flows/README.md`.
- Tham khảo deletion contract hiện có tại
  `docs/api/media-service/endpoints/delete-media-by-id.md` và
  `docs/api/media-service/endpoints/delete-media-usage-by-id.md`; xác minh policy
  của resource mới, không sao chép mặc định.

## Bản đồ project

```text
backend/Services/<Area>/
├── <Service>.Domain/
├── <Service>.Application/
│   ├── Abstractions/
│   ├── Features/<Resources>/Delete/
│   └── DependencyInjection.cs
├── <Service>.Infrastructure/
│   ├── Persistence/
│   └── DependencyInjection.cs
├── <Service>.Api/
│   ├── Endpoints/
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
docs/api/<owning-service>/endpoints/delete-<resource>.md
docs/business-flows/<domain>/<delete-resource>.md
postman/MiniProjectKaopiz.postman_collection.json
```

API doc nằm dưới owner service. Một DELETE endpoint có đúng một API doc và một
business-flow file ánh xạ 1:1.

## Convention trong repository

- ASP.NET Core Minimal APIs, route từ `ApiRoutes`, endpoint extension `Map...`
  được đăng ký trong `Program.cs`.
- Operation metadata dùng `.WithName`, `.WithTags(ServiceNames.<Service>)`,
  `.Produces(StatusCodes.Status204NoContent)` và
  `.Produces<ApiErrorResponse>(...)`.
- Success DELETE thường dùng `Results.NoContent()`; không tạo JSON envelope cho
  `204`.
- Expected error được shared middleware map sang `ApiErrorResponse`.
- Shared transport/dependency error code thuộc `ApiErrorCodes`; business error
  code thuộc owner Application.
- Application sở hữu policy và abstraction; Infrastructure sở hữu EF/MySQL,
  external adapters và transaction mechanics.
- NUnit dùng cho tests. Component test dùng TestServer/shared middleware;
  integration test dùng database/dependency thật, biệt lập.

## Chọn soft delete hay hard delete

### Soft delete

Dùng khi cần audit, retention, restore, tránh phá historical reference hoặc cần
cleanup bất đồng bộ. Phải quyết định:

- marker (`deleted_at`, status, actor/reason, version);
- global/default query visibility và admin/include-deleted path;
- relation/unique index behavior khi record đã xóa;
- API behavior với already-deleted record;
- retention và purge owner;
- event/outbox và cleanup job.

Không chỉ thêm `deleted_at`; mọi query/repository liên quan phải tuân visibility
rule và integration test phải chứng minh điều đó.

### Hard delete

Dùng khi retention/audit cho phép và resource có thể bị xóa vật lý. Phải quyết
định:

- foreign key `CASCADE`, `RESTRICT` hay cleanup có chủ đích;
- thứ tự xóa trong transaction;
- behavior khi dependency xuất hiện đồng thời;
- dữ liệu external như MinIO/cache/search;
- audit/event cần giữ ngoài row bị xóa.

Database constraint là safety net. Không bắt exception thô rồi lộ tên constraint
hoặc schema trong API.

## Idempotency và status

Các lựa chọn hợp lệ phải được chốt trong contract:

- Lần đầu xóa thành công: `204`.
- Gọi lại:
  - `204` nếu API coi “resource không còn tồn tại” là success idempotent; hoặc
  - `404` nếu API cần caller phân biệt target không tồn tại/đã xóa.
- `409` khi resource tồn tại nhưng active dependency, usage, state hoặc
  concurrency token cấm xóa.

Idempotency nói về trạng thái và side effects, không bắt buộc mọi lần gọi có cùng
status. Dù chọn `204` hay `404` cho lần lặp, không được tạo thêm outbox event,
cleanup job, audit record hay external delete operation.

## Side effects và nhất quán

- Ghi state/delete và outbox trong cùng database transaction khi có thể.
- Consumer/cleanup handler phải idempotent; dùng resource ID/event ID làm
  dedupe key khi phù hợp.
- External cleanup thất bại không được phục hồi mù quáng row đã commit; dùng
  retry/dead-letter/runbook theo architecture.
- Xác định event xảy ra lúc marked-deleted hay purged; không phát trùng cả hai nếu
  downstream contract chỉ mong một event.
- `409` phải phản ánh business condition an toàn, không lộ internal relation.

## Lệnh kiểm tra

```bash
dotnet test backend/Services/<Area>/<Service>.UnitTests/<Service>.UnitTests.csproj
dotnet test backend/Services/<Area>/<Service>.IntegrationTests/<Service>.IntegrationTests.csproj
dotnet build
dotnet test
dotnet format --verify-no-changes
```

Với soft delete, integration test phải kiểm tra cả row lưu giữ và query visibility.
Với hard delete, kiểm tra cascade/restrict và rollback trên database thật.
