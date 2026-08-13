# Tham chiếu POST endpoint

## Nguồn bắt buộc

- `AGENTS.md`
- `rules/workflow-skills.md`
- `rules/documentation-language.md`
- `rules/code-quality.md`
- `rules/testing.md`
- `rules/api-documentation.md`
- `rules/messaging.md` nếu action phát message
- `docs/architecture/clean-architecture.md`
- `docs/architecture/service-communication.md` nếu gọi service/message
- `docs/api/README.md`
- `docs/api/_templates/endpoint.md`
- `docs/api/shared/response-format.md`
- `docs/api/shared/error-format.md`
- API, business-flow, database và architecture docs liên quan
- `skills/api-post-endpoint/template.md`
- Các test skill; migration skill nếu schema thay đổi

## Bản đồ project

```text
backend/
├── BuildingBlocks/
│   ├── BuildingBlocks.Contracts/Api/
│   │   ├── ApiRoutes.cs
│   │   ├── ApiConstants.cs
│   │   └── ApiContracts.cs
│   ├── BuildingBlocks.Presentation/
│   │   ├── Api/ApiResponseFactory.cs
│   │   └── Middleware/ApiExceptionHandlingMiddleware.cs
│   ├── BuildingBlocks.Messaging.Abstractions/
│   └── BuildingBlocks.Messaging/
└── Services/<Service>/
    ├── <ServiceName>.Domain/
    ├── <ServiceName>.Application/
    │   ├── Features/<Resources>/Create/
    │   ├── Features/<Actions>/Start/
    │   ├── Abstractions/
    │   └── DependencyInjection.cs
    ├── <ServiceName>.Infrastructure/
    │   ├── Persistence/
    │   ├── Messaging/
    │   └── DependencyInjection.cs
    ├── <ServiceName>.Api/
    │   ├── Contracts/Requests/
    │   ├── Contracts/Responses/
    │   ├── Endpoints/
    │   ├── Mappers/
    │   └── Program.cs
    ├── <ServiceName>.UnitTests/
    └── <ServiceName>.IntegrationTests/

docs/api/<owning-service>/endpoints/post-<resource-or-action>.md
docs/business-flows/<domain>/post-<resource-or-action>.md
docs/database/
docs/tests/
postman/MiniProjectKaopiz.postman_collection.json
```

## Implementation mẫu trong repository

- `201`, `Location`, request/response mapping:
  `backend/Services/Media/MediaService.Api/Endpoints/Media/CreateMediaUsageEndpoint.cs`.
- Multipart `201`:
  `backend/Services/Media/MediaService.Api/Endpoints/Media/UploadMediaEndpoint.cs`.
- Application create handler:
  `backend/Services/Media/MediaService.Application/Features/Usages/Create/`.
- Transaction và duplicate-key mapping:
  `backend/Services/Media/MediaService.Infrastructure/Persistence/EfMediaRepository.cs`.
- Component POST tests:
  `backend/Services/Media/MediaService.UnitTests/Endpoints/MediaCommandEndpointTests.cs`.
- Integration flow:
  `backend/Services/Media/MediaService.IntegrationTests/Flows/MediaUploadUsageFlowTests.cs`.
- API/flow docs:
  `docs/api/media-service/endpoints/post-media-usages.md` và
  `docs/business-flows/media/media-upload-and-usage.md`.

Các mẫu hiện tại có endpoint chưa dùng idempotency key. Endpoint mới phải chốt
policy riêng; không copy thiếu sót đó nếu retry có thể nhân đôi side effect.

## Phân loại POST

### Create đồng bộ — `201 Created`

Dùng khi request hoàn thành việc tạo resource trước khi response:

- durable resource đã tồn tại khi trả response;
- body chứa representation/result trong standard envelope;
- `Location` trỏ đến canonical GET của resource;
- retry/replay không tạo resource thứ hai khi idempotency contract được bật;
- side effects cần thiết để coi là “created” đã hoàn tất hoặc có consistency
  strategy được document.

Không trả `201` nếu chỉ mới enqueue công việc.

### Action bất đồng bộ — `202 Accepted`

Dùng khi server chấp nhận và xử lý sau:

- persist operation/job với ID và initial status trước response;
- body chứa operation reference/status trong standard envelope;
- `Location` trỏ đến endpoint lấy trạng thái operation;
- accepted không có nghĩa completed; doc nêu terminal statuses và failure model;
- durable handoff qua outbox/queue hoặc cơ chế tương đương;
- retry cùng idempotency key trả cùng operation, không enqueue duplicate.

Không dùng `Location` trỏ đến resource chưa chắc sẽ được tạo nếu contract thực
tế theo dõi operation.

## Idempotency contract

POST không idempotent theo HTTP semantics, vì vậy retry-safe behavior phải được
thiết kế:

- Input key: `Idempotency-Key` header hoặc business key ổn định.
- Validation: required/optional, length, charset, entropy.
- Scope: owner service + operation + authenticated actor/tenant.
- Fingerprint: canonical method/path/relevant payload/content hash.
- First request: atomically claim key cùng transaction hoặc reservation.
- Same key, same fingerprint:
  - completed: replay status/body/Location tương đương;
  - processing: trả operation hiện tại hoặc status đã document.
- Same key, different fingerprint: `409 IDEMPOTENCY_KEY_REUSED`.
- Concurrent same key: một winner; request còn lại replay/wait/conflict theo
  contract, không cùng thực thi side effect.
- TTL/retention: dài hơn retry window; cleanup không phá audit/operation cần giữ.
- Failure: phân biệt safe-to-retry với terminal failure.

Không lưu secret trong idempotency record. Không dựa vào in-memory dictionary
cho multi-instance production service.

## Transaction và side effects

- Database constraints là lớp bảo vệ cuối cho uniqueness.
- Map expected duplicate/concurrency exception sang business error, không lộ SQL.
- Resource + idempotency result cần atomic nếu cùng database.
- Operation + outbox + idempotency claim cần atomic với `202`.
- External HTTP/storage side effect không thể nằm trong DB transaction: chốt
  ordering, compensation và retry.
- Message consumer vẫn cần idempotent; producer key không thay thế consumer
  deduplication.
- Chỉ publish sau commit hoặc dùng outbox, tránh publish rồi rollback.

## Route và `Location`

- Route/path builder dùng `ApiRoutes`.
- Direct service path dùng nội bộ; public path qua `GatewayRoutePrefixes` dùng
  khi client gọi qua Gateway.
- `Location` phải ổn định, không chứa hostname phụ thuộc máy và không lộ internal
  key.
- `201`: canonical resource GET path.
- `202`: operation-status GET path.
- Test cả route builder và exact `Location` header.

## Request, response và errors

- API contracts ở `Api/Contracts`; Application command không mang
  `HttpRequest`, header collection hoặc ASP.NET type.
- Validate syntax tại boundary và business invariant tại Application.
- Success JSON dùng `ApiResponse<T>`.
- Expected errors dùng application exception/shared middleware.
- Validation details ở `error.details`.
- Xem xét `400`, `401`, `403`, `404`, `409`, `413`, `415`, `422` chỉ nếu project
  đã chốt, và dependency/`5xx`.

## Tài liệu 1:1

API doc duy nhất phải nêu:

- create hay action; direct/public path và owner;
- auth/ownership;
- content type, header idempotency, payload schema/example;
- `201` hoặc `202` example cùng exact `Location`;
- validation/preconditions/error codes;
- idempotency scope/fingerprint/replay/conflict/retention;
- transaction, concurrency, side effects, outbox/retry/compensation.

Business-flow file duy nhất chỉ mô tả POST đó:

- actor và điều kiện đầu vào;
- first submission;
- duplicate/replay/concurrent submission;
- write/side effect sequence;
- success và failure/compensation;
- dữ liệu thay đổi, operation/event tạo ra.

Không gộp GET detail/list hoặc update/delete vào flow này.

## Postman

- Request nằm trong đúng service folder.
- Method `POST`, content type đúng, full body/form-data.
- Variables cho base URL, IDs và `idempotencyKey`; key được generate khi cần.
- Success test kiểm tra `201` hoặc `202`, envelope và `Location`.
- Lưu resource/operation ID và Location.
- Replay cùng key + payload; xác nhận cùng logical ID và không duplicate.
- Reuse cùng key + payload khác; xác nhận `409`.
- Validation/not-found/business conflict/auth cases.
- Không commit token, credential, object-storage key.

## Coverage checklist

### Unit

- [ ] Payload/header validation và boundary.
- [ ] Authorization/ownership/preconditions.
- [ ] Create/state transition success.
- [ ] Not-found và business conflict.
- [ ] First idempotency claim.
- [ ] Same-key/same-payload replay.
- [ ] Same-key/different-payload conflict.
- [ ] Side effect chỉ chạy một lần.
- [ ] Cancellation behavior.

### Component

- [ ] Binding/content type/header bằng TestServer.
- [ ] Exact `201` hoặc `202`.
- [ ] Standard success envelope và `meta.traceId`.
- [ ] Exact `Location`.
- [ ] Safe error envelope.
- [ ] Replay qua HTTP và endpoint metadata/OpenAPI.

### Integration

- [ ] Migration/container cô lập.
- [ ] Resource/operation/idempotency persistence.
- [ ] Transaction commit và rollback.
- [ ] Unique constraint/concurrent same key.
- [ ] Same key khác payload.
- [ ] Outbox/durable dispatch cho `202`.
- [ ] Retry không nhân đôi row/message/external effect.
- [ ] Compensation/failure state nếu có dependency ngoài.

## Lệnh kiểm tra

```bash
dotnet test backend/Services/<Service>/<ServiceName>.UnitTests/<ServiceName>.UnitTests.csproj
dotnet test backend/Services/<Service>/<ServiceName>.IntegrationTests/<ServiceName>.IntegrationTests.csproj
dotnet build backend/Lms.sln -m:1
dotnet test backend/Lms.sln -m:1
dotnet format backend/Lms.sln --verify-no-changes
```
