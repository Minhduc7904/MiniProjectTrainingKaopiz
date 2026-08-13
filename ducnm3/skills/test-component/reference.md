---
title: Tham chiếu component test
description: Ranh giới TestServer, contract HTTP, DI, middleware, route và test double cho component test ASP.NET Core.
---

# Tham chiếu component test

## 1. Ranh giới component

Component test khởi động đủ ASP.NET Core pipeline để quan sát hành vi HTTP nhưng
thay mọi boundary I/O bằng dependency trong memory.

| Thuộc component test | Không thuộc component test |
| --- | --- |
| Route registration và HTTP method | Domain calculation độc lập |
| Request binding và validation | Repository với MySQL thật |
| Middleware order và exception mapping | SQL migration |
| Success/error response envelope | MinIO/RabbitMQ thật |
| DI registration cần cho endpoint | Gọi downstream service qua network |
| Gateway proxy với fake handler | Docker Compose/Testcontainers |

`HttpClient` do `TestServer` tạo không phải external network: request được xử lý
in-process. Ngược lại, `HttpClient` trỏ tới `localhost`, container mapped port
hoặc URL bên ngoài làm test vượt khỏi ranh giới component.

## 2. Quy tắc chọn project

Từ owner production, ánh xạ sang project:

```text
backend/Services/Student/StudentService.Api/...
-> backend/Services/Student/StudentService.ComponentTests/
   StudentService.ComponentTests.csproj

backend/BuildingBlocks/BuildingBlocks.Presentation/...
-> backend/BuildingBlocks/BuildingBlocks.Presentation.ComponentTests/
   BuildingBlocks.Presentation.ComponentTests.csproj

backend/Gateway/Lms.ApiGateway/...
-> backend/Gateway/Lms.ApiGateway.ComponentTests/
   Lms.ApiGateway.ComponentTests.csproj
```

Không thêm component test mới vào `StudentService.UnitTests`,
`MediaService.UnitTests`, `BuildingBlocks.Presentation.Tests` hoặc project
integration. Test cũ trộn loại không phải tiền lệ cho test mới.

Project component tối thiểu dùng package version hiện có trong repository:

- `Microsoft.AspNetCore.TestHost`
- `Microsoft.NET.Test.Sdk`
- `NUnit`
- `NUnit3TestAdapter`
- `NUnit.Analyzers`
- `coverlet.collector`

Thêm `ProjectReference` tới project API/presentation sở hữu endpoint và contract
cần compile. Không tham chiếu Infrastructure chỉ để dùng repository thật.

## 3. Host tối thiểu nhưng trung thực

Fixture nên:

1. Tạo `WebApplicationBuilder`.
2. Gọi `builder.WebHost.UseTestServer()`.
3. Đăng ký production service extension cần kiểm tra.
4. Override dependency I/O bằng fake trước khi build.
5. Build app.
6. Gọi production middleware extension theo đúng order.
7. Gọi production endpoint mapping extension.
8. `StartAsync()` rồi lấy `GetTestClient()`.
9. Dispose client và app trong teardown.

Host tối thiểu không có nghĩa là mô phỏng lại production route/middleware bằng
code copy. Nếu fixture tự viết lại route, test có thể pass dù production
registration đã hỏng.

Không khởi động toàn bộ application nếu startup kéo real database/broker. Tách
registration extension hoặc override rõ boundary để component vẫn cô lập.

## 4. Contract success

Tùy endpoint, success test cần kiểm tra:

- HTTP method và canonical path.
- Status: `200`, `201`, `202`, `204` theo API contract.
- `Content-Type` và header như correlation, location, cache control.
- Shared response envelope có `data` đúng shape.
- Không lộ field nội bộ như connection string, bucket/object key hoặc secret.
- Serialization của enum, UUID, timestamp và nullability.

Không chỉ assert status nếu endpoint chịu trách nhiệm envelope hoặc payload.
Không snapshot toàn JSON nếu snapshot làm test khó đọc; deserialize contract và
assert field ổn định.

## 5. Contract error

Mỗi error test nên chứng minh:

- Status mapping chính xác.
- Envelope error dùng shape chung.
- `error.code` là constant ổn định.
- Message/detail tuân theo contract; không phụ thuộc text nội bộ nếu contract chỉ
  ổn định error code.
- Không có stack trace, exception type, SQL, host, credential hoặc path máy.
- Correlation ID được giữ hoặc sinh theo middleware contract.

Các nhóm thường cần:

1. Binding/validation lỗi (`400`).
2. Authentication/authorization lỗi nếu component sở hữu policy (`401`/`403`).
3. Business not found/conflict (`404`/`409`).
4. Dependency unavailable (`503`) khi health/adapter mapping thuộc component.
5. Unhandled exception thành safe `500` khi shared error middleware được test.

Không ép mọi endpoint có đủ mọi nhóm; bám API contract thực tế.

## 6. Route và middleware

### Route

- Dùng constant/path builder production trong request.
- Với template, kiểm tra cả parameter hợp lệ và malformed.
- Assert sai HTTP verb trả contract framework mong đợi khi behavior quan trọng.
- Với Gateway, dùng fake downstream `HttpMessageHandler`; không mở listener.

### Middleware

- Giữ order production cho middleware đang test.
- Dựng endpoint nhỏ chỉ để kích hoạt middleware khi middleware là SUT.
- Kiểm tra short-circuit, header propagation và response đã bắt đầu nếu contract
  có liên quan.
- Không gọi middleware method trực tiếp nếu mục tiêu là pipeline behavior.

### DI

- Resolve public service/endpoint dependency qua host.
- Một smoke test startup có giá trị khi mục tiêu là registration graph.
- Không assert toàn bộ service collection như implementation detail.
- Override boundary dependency bằng registration rõ ràng, tránh để production
  client lặng lẽ gọi network.

## 7. Test doubles

Đặt reusable double trong:

```text
<ComponentTests project>/
├── TestDoubles/
│   ├── StubRepository.cs
│   ├── StubHealthProbe.cs
│   └── StubDownstreamHandler.cs
├── Fixtures/
│   └── TestApplication.cs
└── Endpoints/
    └── StudentEndpointComponentTests.cs
```

Test double component có thể mô phỏng state/result ở boundary, nhưng không được
reimplement business logic. Ví dụ:

- Repository stub trả entity hoặc `null`.
- Health probe stub trả healthy/unhealthy.
- Handler giả trả `HttpResponseMessage` cố định.
- Clock giả trả timestamp cố định.

Không dùng EF in-memory như bằng chứng cho SQL/relational behavior. Nếu endpoint
chỉ cần application result, fake repository/application port là đủ. SQL thật
thuộc integration test.

## 8. Lifecycle và isolation

- Tạo host mới mỗi test khi registration/state thay đổi theo scenario.
- Có thể dùng one-time fixture nếu host hoàn toàn immutable; reset mọi fake state
  trong `[SetUp]`.
- Dùng `TestContext.CurrentContext.CancellationToken` cho async operation.
- Dispose response, request content, client và app.
- Không dùng static mutable collection cho request history nếu test chạy song song.
- Không đánh dấu `[NonParallelizable]` để che shared state có thể loại bỏ.

## 9. Quy ước tên

```text
Class:  <EndpointOrMiddleware>ComponentTests
Method: <HttpMethodOrBehavior>_<Scenario>_<ExpectedContract>
```

Ví dụ:

- `Get_StudentExists_ReturnsOkEnvelope`
- `Get_InvalidStudentId_ReturnsValidationEnvelope`
- `InvokeAsync_UnhandledException_ReturnsSafeInternalError`
- `MapRoutes_ServicePrefixConfigured_RegistersExpectedRoute`
- `Post_DependencyUnavailable_ReturnsServiceUnavailableEnvelope`

## 10. Catalog tài liệu

`docs/tests/<owner>/component.md` cần ghi:

- Exact project và source fixture.
- Endpoint/middleware/route được kiểm tra.
- Dependency nào được thay bằng in-memory double.
- Request gồm method/path/body/header quan trọng.
- Expected status, header, success/error envelope.
- Lệnh chạy.

Khi tạo trang mới, thêm link vào `docs/tests/README.md`. Không đưa component
coverage vào `unit.md` hoặc `integration.md`.

## 11. Verification

Chạy theo thứ tự:

```bash
dotnet test <exact-component-project>.csproj --filter "FullyQualifiedName~<FixtureName>"
dotnet test <exact-component-project>.csproj
dotnet format <exact-component-project>.csproj --verify-no-changes
```

Chạy solution test nếu thêm project, đổi shared middleware, route constant hoặc
DI extension:

```bash
dotnet test backend/Lms.sln -m:1
```

Nếu test cố kết nối database/network, đó là lỗi isolation, không phải lý do để
khởi động Compose. Thay đúng boundary hoặc chuyển behavior sang integration test.

## 12. Anti-pattern

- Chỉ gọi endpoint delegate trực tiếp nhưng gọi đó là component test.
- Copy production mapping vào fixture thay vì gọi extension thật.
- Hard-code route trong test khi project có route constant.
- Chỉ assert status, bỏ qua envelope/error code.
- Dùng local developer database hoặc service URL.
- Mở `WebApplication.Run()`/Kestrel port.
- Nhét Testcontainers vào component fixture.
- Đặt TestServer test trong `*.UnitTests`.
