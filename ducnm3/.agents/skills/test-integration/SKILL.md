---
name: test-integration
description: Thêm hoặc cập nhật NUnit integration test bằng Testcontainers cho MySQL, MinIO, RabbitMQ và boundary thật.
---

# Integration test

## Khi nào dùng

Dùng skill này khi hành vi chỉ có thể chứng minh với adapter/resource thật:

- SQL migration thật trên MySQL.
- Repository/DbContext và relational constraint/transaction.
- MinIO adapter, bucket/object lifecycle và metadata.
- RabbitMQ/MassTransit topology, routing, retry, error queue và correlation.
- Giao tiếp giữa application adapter với dependency container cô lập.

Mọi resource phải do Testcontainers tạo riêng cho test suite. Không bao giờ dùng
developer Docker Compose, database/bucket/queue dùng chung, localhost service có
sẵn hoặc credential từ môi trường phát triển.

## Cổng bắt buộc trước khi chỉnh sửa

Trước khi sửa bất kỳ file nào:

1. Đọc toàn bộ `.agents/skills/test-integration/reference.md`.
2. Đọc toàn bộ `.agents/skills/test-integration/template.md`.
3. Đọc `rules/testing.md`, `rules/code-quality.md`,
   `rules/documentation-language.md`, `rules/messaging.md`,
   `docs/development/testing.md` và tài liệu database/API liên quan.
4. Đọc migration thật, repository/adapter registration, integration test hiện
   có và trang `docs/tests/<owner>/integration.md`.
5. Chốt exact project, resource, image và migration entry point. Không chỉnh sửa
   khi đường dẫn vẫn chứa placeholder.

## Chọn chính xác project

Chọn theo owner của boundary:

- Service:
  `backend/Services/<Service>/<Service>Service.IntegrationTests/<Service>Service.IntegrationTests.csproj`.
- Building block:
  `backend/BuildingBlocks/<Component>.IntegrationTests/<Component>.IntegrationTests.csproj`.
- Tool:
  `backend/Tools/<Tool>.IntegrationTests/<Tool>.IntegrationTests.csproj`.
- Luồng cross-service không thuộc một owner:
  `tests/<Flow>.IntegrationTests/<Flow>.IntegrationTests.csproj`.

Nếu project chuyên biệt đã có, dùng project đó. Nếu chưa có, chỉ tạo project khi
resource thật là điều kiện cần để kiểm tra behavior; thêm project vào
`backend/Lms.sln` nếu project nằm dưới `backend/`. Không đặt integration test
trong `*.UnitTests`, `*.ComponentTests` hoặc project đa loại.

Trước khi chỉnh sửa, ghi lại:

```text
Boundary:
Production owner:
Project test: <đường dẫn .csproj chính xác>
File test: <đường dẫn .cs chính xác>
Catalog: docs/tests/<owner>/integration.md
Containers/images:
Migration source/runner:
Isolation key:
Cleanup strategy:
```

## Quy trình

1. Chỉ chọn resource thật cần cho behavior: MySQL, MinIO, RabbitMQ hoặc tổ hợp
   tối thiểu. Boundary còn lại phải dùng fake/in-memory dependency.
2. Dùng Testcontainers builder với image tag rõ ràng và credential chỉ dành cho
   test. Lấy host/mapped port/connection string từ container object.
3. Start container trong `[OneTimeSetUp]` hoặc fixture lifecycle phù hợp; đăng ký
   cleanup ngay bằng `[OneTimeTearDown]` và `DisposeAsync`.
4. Với database, áp dụng chính các file SQL migration production theo đúng thứ
   tự bằng migration runner thật. Không thay bằng `EnsureCreated`, schema copy
   hoặc SQL rút gọn viết trong test.
5. Tạo database/schema, bucket, object prefix, virtual host hoặc queue name cô
   lập. Mỗi test tự tạo data và dọn state; không phụ thuộc thứ tự.
6. Dùng repository/adapter/communication registration production. Chỉ fake
   boundary không thuộc mục tiêu kiểm tra.
7. Viết success và failure/constraint/lifecycle contract có giá trị. Với async
   messaging, chờ bằng bounded polling/signal và timeout rõ ràng, không sleep mù.
8. Đặt class `<Boundary>IntegrationTests`; đặt method
   `<Operation>_<Scenario>_<ExpectedResult>`.
9. Cập nhật `docs/tests/<owner>/integration.md`; nếu tạo catalog mới, thêm link
   vào `docs/tests/README.md`.
10. Chạy verification và xác nhận resource được dọn kể cả khi test thất bại.

## Lệnh

Xác nhận Docker Engine:

```bash
docker info
```

Chạy fixture:

```bash
dotnet test <exact-integration-project>.csproj --filter "FullyQualifiedName~<FixtureName>"
```

Chạy project và format:

```bash
dotnet test <exact-integration-project>.csproj
dotnet format <exact-integration-project>.csproj --verify-no-changes
```

Khi đổi migration/shared adapter hoặc thêm project:

```bash
dotnet test backend/Lms.sln -m:1
```

Không chạy `docker compose up` để chuẩn bị integration test.

## Verification bắt buộc

- Container do fixture tạo, không trỏ tới Compose/shared MySQL, MinIO hoặc
  RabbitMQ.
- Image có tag rõ ràng; endpoint/credential lấy từ container.
- MySQL test áp dụng migration SQL production thật và fail nếu migration lỗi.
- Repository test xác minh relational behavior, constraint, transaction hoặc
  mapping mà unit test không chứng minh được.
- MinIO test tạo bucket/prefix riêng và dọn object/bucket hoặc dispose container.
- RabbitMQ test dùng queue/exchange/consumer identity riêng, bounded wait và kiểm
  tra routing/retry/error/correlation theo contract.
- Start/stop/dispose nằm trong lifecycle hook; cleanup chạy trong `finally` hoặc
  teardown an toàn.
- Test độc lập, không dựa thứ tự, state hoặc resource của test khác.
- Tên test và catalog integration đúng source; không trộn unit/component test.

## Điều kiện dừng

Nếu Docker không khả dụng, báo rõ verification chưa chạy; không chuyển sang
developer Compose hoặc shared resource. Nếu behavior không cần resource thật,
chuyển về `.agents/skills/test-unit` hoặc `.agents/skills/test-component`.
