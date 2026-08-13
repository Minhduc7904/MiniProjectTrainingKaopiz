---
title: Template component test
description: Fixture TestServer, success/error contract và checklist component test ASP.NET Core.
---

# Template component test

## Phiếu chốt phạm vi

```text
HTTP component:
Production owner:
Exact component project:
Exact fixture file:
Catalog: docs/tests/<owner>/component.md
Production registration extensions:
Method/path:
Success contract:
Error contracts:
Boundary dependencies cần thay:
Focused test command:
```

Nếu cần database/broker/storage thật, dùng integration skill. Nếu không cần HTTP
pipeline, dùng unit skill.

## Cấu trúc thư mục

```text
<Owner>.ComponentTests/
├── Endpoints/
│   └── EntityEndpointComponentTests.cs
├── Middleware/
├── TestDoubles/
│   └── StubEntityRepository.cs
├── Fixtures/
│   └── TestApplication.cs
└── <Owner>.ComponentTests.csproj
```

## Fixture TestServer

```csharp
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Owner.ComponentTests.Endpoints;

public sealed class EntityEndpointComponentTests
{
    private static readonly Guid EntityId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private WebApplication app = null!;
    private HttpClient client = null!;
    private StubEntityRepository repository = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        repository = new StubEntityRepository();

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddOwnerApplication();
        builder.Services.AddSingleton<IEntityRepository>(repository);

        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapEntityEndpoints();

        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task Get_EntityExists_ReturnsOkEnvelope()
    {
        // Arrange
        repository.Result = new EntityResponse(EntityId, "ACTIVE");

        // Act
        using var response = await client.GetAsync(
            ApiRoutes.Entities.GetByIdServicePath(EntityId),
            TestContext.CurrentContext.CancellationToken);
        var envelope =
            await response.Content.ReadFromJsonAsync<ApiResponse<EntityResponse>>(
                TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(
                response.Content.Headers.ContentType?.MediaType,
                Is.EqualTo("application/json"));
            Assert.That(envelope, Is.Not.Null);
            Assert.That(envelope!.Data.Id, Is.EqualTo(EntityId));
            Assert.That(envelope.Data.Status, Is.EqualTo("ACTIVE"));
        });
    }

    [Test]
    public async Task Get_EntityMissing_ReturnsNotFoundEnvelope()
    {
        // Arrange
        repository.Result = null;

        // Act
        using var response = await client.GetAsync(
            ApiRoutes.Entities.GetByIdServicePath(EntityId),
            TestContext.CurrentContext.CancellationToken);
        var envelope =
            await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
                TestContext.CurrentContext.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(
            TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(envelope?.Error.Code, Is.EqualTo(EntityErrorCodes.NotFound));
            Assert.That(body, Does.Not.Contain("System."));
            Assert.That(body, Does.Not.Contain("stack"));
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }
}
```

Thay type và extension minh họa bằng identifier production. Không copy route,
middleware hoặc endpoint implementation vào fixture.

## Stub boundary

```csharp
namespace Owner.ComponentTests.TestDoubles;

internal sealed class StubEntityRepository : IEntityRepository
{
    public EntityResponse? Result { get; set; }

    public Task<EntityResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        Task.FromResult(Result);
}
```

Stub chỉ điều khiển boundary result. Business mapping vẫn phải chạy qua code
production.

## Ma trận contract

```text
Success:
- method/path:
- request headers/body:
- expected status:
- expected envelope.data:
- expected headers:

Error:
- trigger:
- expected status:
- expected error.code:
- expected correlation/header:
- dữ liệu tuyệt đối không được lộ:
```

## Mẫu catalog

````markdown
## Phạm vi

Dự án: `backend/.../<Owner>.ComponentTests/<Owner>.ComponentTests.csproj`
Mã nguồn: `Endpoints/<Endpoint>ComponentTests.cs`
Dependency: ASP.NET Core `TestServer`; repository/downstream là stub trong memory.

Chạy:

```bash
dotnet test backend/.../<Owner>.ComponentTests/<Owner>.ComponentTests.csproj
```

## Ca kiểm thử

- `<Verb>_<Scenario>_<ExpectedContract>`: gửi `<method/path/body>`; đạt khi status
  là `<status>`, envelope có `<data hoặc error.code>` và không lộ `<sensitive data>`.
````

## Checklist trước bàn giao

### Phân loại và project

- [ ] Test cần HTTP pipeline, route, middleware, envelope hoặc DI.
- [ ] Exact project là `*.ComponentTests`.
- [ ] Không đặt test vào `*.UnitTests`, `*.IntegrationTests` hoặc project đa loại.
- [ ] Không có real DB, Docker, Testcontainers, network hoặc shared resource.

### Host và contract

- [ ] Host dùng `UseTestServer()` và `GetTestClient()`.
- [ ] Fixture gọi production registration/middleware/route extension.
- [ ] Boundary I/O được thay bằng test double trong memory.
- [ ] Success test assert status, payload/envelope và header quan trọng.
- [ ] Error test assert status, stable `error.code` và safe response.
- [ ] Request dùng production route constant.
- [ ] Client, response, content và app được dispose.

### Tên và tài liệu

- [ ] Class dùng `<EndpointOrMiddleware>ComponentTests`.
- [ ] Method dùng `<HttpMethodOrBehavior>_<Scenario>_<ExpectedContract>`.
- [ ] `docs/tests/<owner>/component.md` đã cập nhật.
- [ ] `docs/tests/README.md` có link nếu catalog mới.

### Verification

- [ ] Focused fixture pass.
- [ ] Exact component project pass.
- [ ] Format verification pass.
- [ ] Solution pass khi thêm project hoặc đổi shared route/presentation/DI.
