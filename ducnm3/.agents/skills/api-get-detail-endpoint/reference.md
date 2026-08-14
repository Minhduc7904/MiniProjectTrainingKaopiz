# Tham chiếu GET detail endpoint

## Nguồn bắt buộc

Đọc các file này trước khi triển khai:

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
- API doc, business flow và database doc của resource đang sửa
- `.agents/skills/api-get-detail-endpoint/template.md`
- Ba test skill tương ứng; thêm migration skill nếu schema thay đổi

## Bản đồ project

Thay `<Service>` bằng `Course`, `Student`, `Media`, `Notification` hoặc
`Scheduler`; thay `<ServiceName>` bằng prefix project thực tế.

```text
backend/
├── BuildingBlocks/
│   ├── BuildingBlocks.Contracts/Api/
│   │   ├── ApiRoutes.cs
│   │   ├── ApiConstants.cs
│   │   └── ApiContracts.cs
│   └── BuildingBlocks.Presentation/
│       ├── Api/ApiResponseFactory.cs
│       └── Middleware/ApiExceptionHandlingMiddleware.cs
└── Services/<Service>/
    ├── <ServiceName>.Domain/
    ├── <ServiceName>.Application/
    │   ├── Features/<Resources>/GetById/
    │   ├── Abstractions/
    │   └── DependencyInjection.cs
    ├── <ServiceName>.Infrastructure/
    │   ├── Persistence/
    │   ├── Clients/
    │   └── DependencyInjection.cs
    ├── <ServiceName>.Api/
    │   ├── Contracts/Responses/
    │   ├── Endpoints/
    │   ├── Mappers/
    │   └── Program.cs
    ├── <ServiceName>.UnitTests/
    └── <ServiceName>.IntegrationTests/
```

Tài liệu và collection:

```text
docs/api/<owning-service>/endpoints/get-<resource>-by-id.md
docs/business-flows/<domain>/get-<resource>-by-id.md
docs/database/
docs/tests/
postman/MiniProjectKaopiz.postman_collection.json
```

## Implementation mẫu trong repository

- Route dùng chung và path builder:
  `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`.
- GET detail hoàn chỉnh:
  `backend/Services/Student/StudentService.Api/Endpoints/StudentEndpoints.cs`.
- Application handler và error:
  `backend/Services/Student/StudentService.Application/Features/Students/GetById/`.
- EF adapter read-only:
  `backend/Services/Student/StudentService.Infrastructure/Persistence/EfStudentRepository.cs`.
- Đăng ký feature/adapter:
  `StudentService.Application/DependencyInjection.cs` và
  `StudentService.Infrastructure/DependencyInjection.cs`.
- Component test bằng TestServer:
  `backend/Services/Student/StudentService.UnitTests/StudentEndpointTests.cs`.
- Unit test handler:
  `backend/Services/Student/StudentService.UnitTests/GetStudentByIdHandlerTests.cs`.
- API doc tham khảo:
  `docs/api/student-service/endpoints/get-student-by-id.md`.

Các file mẫu phản ánh convention hiện tại, không phải lý do để bỏ qua contract
mới hoặc copy tên `Student` sang service khác.

## Convention quan trọng

### Ranh giới Clean Architecture

- `Api` bind HTTP, map request/response và gọi handler.
- `Application` sở hữu use case, abstraction, validation, authorization rule và
  business error.
- `Infrastructure` implement EF/MySQL, typed HTTP client, storage hoặc cache
  adapter.
- `Domain` không phụ thuộc layer khác.
- API không gọi `DbContext`; Application không tham chiếu ASP.NET Core/EF Core.

### Route và constants

- Route nằm trong `ApiRoutes`; endpoint, client và test không lặp magic path.
- Khi có Gateway, cung cấp service path và public path builder.
- Dùng `ServiceNames`, `GatewayRoutePrefixes`, `ApiHeaderNames`,
  `ApiErrorCodes` hiện có.
- Shared HTTP header mới chỉ đặt trong `BuildingBlocks.Contracts` khi thực sự
  dùng chung; business constants ở service sở hữu.

### Contract GET detail

- Method duy nhất: `GET`.
- Path nhận ID rõ kiểu và tên; GET không có body.
- `200 OK`: `ApiResponse<T>` với `data` và `meta.traceId`.
- `404`: resource không tồn tại hoặc không được phép tiết lộ theo policy đã chốt.
- `400`: ID/query/header không hợp lệ.
- GET phải safe, idempotent và không ghi database, publish message hoặc tạo job.
- Ghi rõ direct service path và public Gateway path.

### Cache

Chọn một policy có chủ đích:

1. `Cache-Control: no-store` cho dữ liệu nhạy cảm, phụ thuộc quyền hoặc chưa có
   invalidation policy.
2. `Cache-Control: private, max-age=<seconds>` cho dữ liệu theo user.
3. `Cache-Control: public, max-age=<seconds>` chỉ khi representation công khai,
   không phụ thuộc actor và có invalidation/staleness chấp nhận được.
4. ETag: sinh validator ổn định từ version/updated timestamp/representation;
   so sánh `If-None-Match`, trả `304` không body khi khớp và luôn nêu
   `Cache-Control`.

Không cache `404` trừ khi contract ghi rõ negative caching. Thêm `Vary` khi
representation thay đổi theo header. Không dùng ETag dựa trên giá trị bí mật.

### Query adapter

- Dùng `AsNoTracking()` cho EF read.
- Filter theo ID, tenant/visibility/soft-delete trước khi project.
- `Select` đúng fields response; tránh load aggregate hoặc relation không dùng.
- Không dùng lazy loading và không tạo N+1.
- Không truy vấn database thuộc service khác.

### Error envelope

`ApiExceptionHandlingMiddleware` map `ApiException` sang:

```json
{
  "error": {
    "code": "RESOURCE_NOT_FOUND",
    "message": "Resource not found.",
    "details": []
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

Error code ổn định; message an toàn; field validation nằm trong
`error.details`. Không trả exception text nội bộ.

## Tài liệu 1:1

API doc duy nhất phải theo `docs/api/_templates/endpoint.md` và nêu:

- mục đích, owner, direct/public path;
- authentication, authorization/ownership;
- path parameter; xác nhận không query/body nếu đúng;
- schema và ví dụ `200`;
- `400`, `401`, `403`, `404`, dependency/`5xx` thực tế;
- safe/idempotent/read-only behavior;
- cache policy, ETag/`304` nếu có;
- dependency reads và không có side effect.

Business-flow file duy nhất phải chỉ mô tả endpoint này:

- actor và điều kiện đầu vào;
- luồng đọc chính;
- visibility/not-found;
- cache/conditional request;
- lỗi;
- dữ liệu chỉ đọc và xác nhận không thay đổi.

Không gộp create/update/list vào cùng flow.

## Postman

Cập nhật đúng folder service trong
`postman/MiniProjectKaopiz.postman_collection.json`:

- URL dùng collection variables cho base URL và resource ID.
- Method `GET`, không có `body`.
- Header auth/correlation/`If-None-Match` khi contract yêu cầu.
- Test `200`, envelope, ID/fields và cache header.
- Case ID sai, `404`, unauthorized/forbidden nếu có.
- Nếu ETag: lưu ETag từ response rồi gửi conditional request và kiểm tra `304`
  không body.
- Không đưa token, credential hoặc ID phụ thuộc máy vào collection.

## Coverage checklist

### Unit

- [ ] ID hợp lệ và ID rỗng/sai.
- [ ] Found trả projection đúng.
- [ ] Missing/hidden trả đúng error.
- [ ] Authorization/visibility rule.
- [ ] Cancellation được truyền.
- [ ] Không gọi write/publish adapter.

### Component

- [ ] Route và binding thật bằng TestServer.
- [ ] GET không body, `200` envelope và `meta.traceId`.
- [ ] `400`/`404`/auth error envelope an toàn.
- [ ] Cache-Control, Vary, ETag/`304` đúng contract.
- [ ] Endpoint metadata `.Produces...` đúng.

### Integration

- [ ] Migration/container cô lập nếu dùng database thật.
- [ ] Found/not-found/soft-delete/tenant visibility.
- [ ] Projection và relation không gây N+1.
- [ ] Read không thay đổi row/version/audit.
- [ ] Query plan/index được kiểm tra nếu endpoint nhạy hiệu năng.

## Lệnh kiểm tra

```bash
dotnet test backend/Services/<Service>/<ServiceName>.UnitTests/<ServiceName>.UnitTests.csproj
dotnet test backend/Services/<Service>/<ServiceName>.IntegrationTests/<ServiceName>.IntegrationTests.csproj
dotnet build backend/Lms.sln -m:1
dotnet test backend/Lms.sln -m:1
dotnet format backend/Lms.sln --verify-no-changes
```

Chỉ chạy integration project nếu project tồn tại và ghi rõ dependency Docker.
Mọi ngoại lệ coverage phải có lý do trong test documentation.
