---
name: test-component
description: Thêm hoặc cập nhật ASP.NET Core component test bằng TestServer cho endpoint, middleware, envelope, DI và route.
---

# Component test

## Khi nào dùng

Dùng skill này để kiểm tra một ASP.NET Core component chạy in-process qua
`TestServer`:

- Endpoint binding, route, status code và header.
- Middleware ordering, correlation, exception mapping và response envelope.
- DI registration và application startup tối thiểu.
- Gateway route/proxy contract với downstream handler giả.

Không dùng database thật, Docker, Testcontainers, port TCP thật hoặc external
network. Repository, health probe, storage, broker và downstream HTTP phải được
thay bằng test double trong memory.

## Cổng bắt buộc trước khi chỉnh sửa

Trước khi sửa bất kỳ file nào:

1. Đọc toàn bộ `skills/test-component/reference.md`.
2. Đọc toàn bộ `skills/test-component/template.md`.
3. Đọc `rules/testing.md`, `rules/code-quality.md`,
   `rules/documentation-language.md`, `rules/api-documentation.md`,
   `docs/development/testing.md` và API contract liên quan trong `docs/api/`.
4. Đọc `Program.cs`, registration extension, route constants, middleware,
   endpoint và component test hiện có của owner.
5. Chốt đường dẫn project cụ thể theo mục dưới đây. Không chỉnh sửa khi đường
   dẫn vẫn chứa placeholder.

## Chọn chính xác project

Component test phải ở project riêng theo owner:

- Service:
  `backend/Services/<Service>/<Service>Service.ComponentTests/<Service>Service.ComponentTests.csproj`.
- Building block:
  `backend/BuildingBlocks/<Component>.ComponentTests/<Component>.ComponentTests.csproj`.
- API Gateway:
  `backend/Gateway/Lms.ApiGateway.ComponentTests/Lms.ApiGateway.ComponentTests.csproj`.

Nếu project chuyên biệt đã tồn tại, dùng project đó. Nếu chưa có và behavior cần
component coverage, tạo đúng project `*.ComponentTests`, thêm nó vào
`backend/Lms.sln`, và tham chiếu owner/API cần thiết. Không thêm component test
mới vào `*.UnitTests`, `*.IntegrationTests` hoặc project test đa loại.

Trước khi chỉnh sửa, ghi lại:

```text
HTTP component:
Production owner:
Project test: <đường dẫn .csproj chính xác>
File test: <đường dẫn .cs chính xác>
Catalog: docs/tests/<owner>/component.md
Routes/contracts:
```

## Quy trình

1. Chọn contract quan sát được: method/path, binding, status, header, envelope
   success và envelope error.
2. Dựng `WebApplication` tối thiểu bằng `UseTestServer`; đăng ký cùng production
   extension và middleware order cần kiểm tra.
3. Override boundary dependency bằng fake/stub trong memory. Không gọi
   `Program.Main`, Compose, database, broker, storage hoặc service thật nếu làm
   mất tính cô lập.
4. Gửi request bằng `app.GetTestClient()` và route constant production; không
   hard-code path khi đã có shared route constant.
5. Viết ít nhất một success contract và các error contract có thể quan sát:
   validation, not found/business error, dependency failure hoặc unhandled error
   nếu middleware sở hữu mapping đó.
6. Assert status, `Content-Type`, header quan trọng và cấu trúc envelope. Với
   error, assert stable error code và không lộ stack trace/secret.
7. Đặt class `<EndpointOrMiddleware>ComponentTests`; đặt method
   `<HttpMethodOrBehavior>_<Scenario>_<ExpectedContract>`.
8. Dispose `HttpClient` và `WebApplication` trong teardown kể cả khi test lỗi.
9. Cập nhật `docs/tests/<owner>/component.md`; nếu tạo trang mới, thêm link vào
   `docs/tests/README.md`.
10. Chạy verification.

## Lệnh

Chạy fixture:

```bash
dotnet test <exact-component-project>.csproj --filter "FullyQualifiedName~<FixtureName>"
```

Chạy project:

```bash
dotnet test <exact-component-project>.csproj
```

Kiểm tra format:

```bash
dotnet format <exact-component-project>.csproj --verify-no-changes
```

Khi tạo project hoặc đổi shared presentation/route:

```bash
dotnet test backend/Lms.sln -m:1
```

## Verification bắt buộc

- Request đi qua `TestServer`, không bind port thật.
- Không có real DB, Testcontainers, Docker, filesystem dùng chung hoặc external
  network.
- DI graph và middleware/route cần kiểm tra được dựng bằng production extension
  tương ứng.
- Success contract assert đúng status, envelope/data và header quan trọng.
- Error contract assert đúng status, `error.code`, envelope và dữ liệu nhạy cảm
  không bị lộ.
- Route test dùng route constant hoặc kiểm tra trực tiếp registration production.
- Test độc lập, chạy được riêng và dispose host/client đúng cách.
- Tên test đúng convention và catalog phản ánh đúng source.
- Diff không chứa unit/integration test trong component project.

## Điều kiện dừng

Nếu mục tiêu chỉ là business logic không có HTTP pipeline, dùng
`skills/test-unit`. Nếu cần xác minh SQL migration, repository, MinIO, RabbitMQ
hoặc adapter với resource thật, dùng `skills/test-integration`.
