---
title: Template integration test
description: Fixture Testcontainers, migration, resource isolation và checklist integration test.
---

# Template integration test

## Phiếu chốt phạm vi

```text
Boundary:
Production owner:
Exact integration project:
Exact fixture file:
Catalog: docs/tests/<owner>/integration.md
Testcontainers modules:
Pinned images:
Production migration source/runner:
Production adapter/repository:
Isolation key:
Per-test cleanup:
Suite cleanup:
Focused test command:
```

Không điền host/port của Compose hoặc shared resource.

## Cấu trúc thư mục

```text
<Owner>.IntegrationTests/
├── Database/
│   ├── MigrationIntegrationTests.cs
│   └── RepositoryIntegrationTests.cs
├── Storage/
│   └── MinioStorageIntegrationTests.cs
├── Messaging/
│   └── RabbitMqIntegrationTests.cs
├── Fixtures/
│   └── IntegrationFixture.cs
└── <Owner>.IntegrationTests.csproj
```

Chỉ tạo thư mục cho boundary thực sự có test.

## Fixture MySQL với migration thật

```csharp
using NUnit.Framework;
using Testcontainers.MySql;

namespace Owner.IntegrationTests.Database;

[NonParallelizable]
public sealed class RepositoryIntegrationTests
{
    private MySqlContainer mysql = null!;
    private IRepository repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        mysql = new MySqlBuilder("mysql:<pinned-version>")
            .WithDatabase("owner_test")
            .WithUsername("owner_test")
            .WithPassword("owner_test_password")
            .Build();

        try
        {
            await mysql.StartAsync();

            var migrationRunner = CreateProductionMigrationRunner(
                mysql.GetConnectionString());
            await migrationRunner.ApplyAsync(
                TestContext.CurrentContext.CancellationToken);

            repository = CreateProductionRepository(mysql.GetConnectionString());
        }
        catch
        {
            await mysql.DisposeAsync();
            throw;
        }
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        await DeleteOwnedRowsAsync(
            TestContext.CurrentContext.CancellationToken);
    }

    [Test]
    public async Task SaveAsync_ValidEntity_PersistsExpectedMapping()
    {
        // Arrange
        var entity = CreateEntityWithFixedId();

        // Act
        await repository.SaveAsync(
            entity,
            TestContext.CurrentContext.CancellationToken);

        // Assert
        var persisted = await LoadWithIndependentQueryAsync(
            entity.Id,
            TestContext.CurrentContext.CancellationToken);
        Assert.Multiple(() =>
        {
            Assert.That(persisted, Is.Not.Null);
            Assert.That(persisted!.Status, Is.EqualTo(entity.Status));
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await DisposeRepositoryAsync();
        await mysql.DisposeAsync();
    }
}
```

Thay helper minh họa bằng migration runner/repository thật. Không thay
`ApplyAsync` bằng `EnsureCreated` hoặc SQL viết lại trong fixture.

## Link migration production vào output

```xml
<ItemGroup>
  <Content
    Include="..\Owner.Infrastructure\Database\Migrations\*.sql"
    Link="Database\Migrations\%(Filename)%(Extension)"
    CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Đường dẫn phải trỏ tới migration production của owner và runner phải đọc đúng
output này theo thứ tự version.

## Fixture MinIO

```csharp
private MinioContainer minio = null!;
private IMinioClient client = null!;
private string bucketName = null!;

[OneTimeSetUp]
public async Task StartMinioAsync()
{
    minio = new MinioBuilder("minio/minio:<pinned-version>").Build();

    try
    {
        await minio.StartAsync();
        client = CreateClientFrom(minio);
        bucketName = $"owner-test-{Guid.NewGuid():N}";
        await CreateBucketAsync(client, bucketName);
    }
    catch
    {
        await minio.DisposeAsync();
        throw;
    }
}

[TearDown]
public Task CleanObjectsAsync() =>
    DeleteObjectsWithPrefixAsync(
        client,
        bucketName,
        TestContext.CurrentContext.Test.ID);

[OneTimeTearDown]
public async Task StopMinioAsync()
{
    await DeleteBucketIfExistsAsync(client, bucketName);
    client.Dispose();
    await minio.DisposeAsync();
}
```

Object key của từng test phải có prefix riêng. Adapter production nhận
endpoint/credential từ `minio`, không từ environment developer.

## Fixture RabbitMQ

```csharp
private RabbitMqContainer rabbitMq = null!;

[OneTimeSetUp]
public async Task StartRabbitMqAsync()
{
    rabbitMq = new RabbitMqBuilder("rabbitmq:<pinned-version>")
        .WithUsername("owner_test")
        .WithPassword("owner_test_password")
        .Build();
    await rabbitMq.StartAsync();
}

private IConfiguration CreateConfiguration(string runId) =>
    new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Messaging:RabbitMq:Host"] = rabbitMq.Hostname,
            ["Messaging:RabbitMq:Port"] =
                rabbitMq.GetMappedPublicPort(5672).ToString(),
            ["Messaging:RabbitMq:Username"] = "owner_test",
            ["Messaging:RabbitMq:Password"] = "owner_test_password",
            ["Messaging:TestRunId"] = runId,
        })
        .Build();

[OneTimeTearDown]
public Task StopRabbitMqAsync() => rabbitMq.DisposeAsync().AsTask();
```

Chờ message bằng signal có timeout:

```csharp
await consumed.Task.WaitAsync(TimeSpan.FromSeconds(30));
```

Không dùng `Task.Delay` cố định thay cho điều kiện hoàn tất. Stop host/bus trước
`StopRabbitMqAsync`.

## Ma trận behavior

```text
Migration:
- database state ban đầu:
- production migration files:
- expected schema/constraint/version:

Repository:
- rows/state ban đầu:
- operation:
- expected rows/constraint/transaction:

MinIO:
- bucket/object prefix:
- operation:
- expected bytes/metadata/existence:

RabbitMQ:
- endpoint identity:
- publish/send:
- expected consumer/retry/error queue/correlation:
```

## Mẫu catalog

````markdown
## Phạm vi

Dự án: `backend/.../<Owner>.IntegrationTests/<Owner>.IntegrationTests.csproj`
Mã nguồn: `<Boundary>/<Fixture>IntegrationTests.cs`
Dependency: `<image tag>` qua Testcontainers; không dùng developer Compose.
Migration: `<production migration path/runner>`.

Chạy:

```bash
docker info
dotnet test backend/.../<Owner>.IntegrationTests/<Owner>.IntegrationTests.csproj
```

## Vòng đời

Fixture tạo `<resource/isolation key>`, áp dụng migration/bootstrap thật, dọn
state theo test và dispose client/container sau suite.

## Ca kiểm thử

- `<Operation>_<Scenario>_<ExpectedResult>`: với `<initial state>`, khi
  `<operation>` thì `<database/object/message state>` phải đúng.
````

## Checklist trước bàn giao

### Phân loại và isolation

- [ ] Behavior cần semantics thật của MySQL, MinIO, RabbitMQ hoặc adapter.
- [ ] Exact project là `*.IntegrationTests`.
- [ ] Không trộn unit/component test.
- [ ] Không dùng Compose, localhost cố định, shared database/bucket/queue.
- [ ] Image được pin tag; credential chỉ dành cho test.

### Lifecycle

- [ ] Container được start trong setup và dispose trong teardown.
- [ ] Startup failure vẫn cleanup container đã tạo.
- [ ] Mỗi test có row/object/queue identity riêng.
- [ ] Data/resource được dọn mà không ảnh hưởng test khác.
- [ ] Async wait có signal/polling bounded, không sleep mù.

### Boundary

- [ ] MySQL dùng SQL migration và migration runner production thật.
- [ ] Repository/adapter/communication registration production được dùng.
- [ ] MinIO lifecycle assert bytes, metadata hoặc existence theo contract.
- [ ] RabbitMQ assert routing/retry/error/correlation theo contract.
- [ ] Boundary không thuộc mục tiêu được thay bằng fake tối thiểu.

### Tên, tài liệu và verification

- [ ] Class dùng `<Boundary>IntegrationTests`.
- [ ] Method dùng `<Operation>_<Scenario>_<ExpectedResult>`.
- [ ] `docs/tests/<owner>/integration.md` và README catalog đã cập nhật.
- [ ] `docker info` thành công.
- [ ] Focused fixture, exact project và format verification pass.
- [ ] Solution pass khi đổi shared migration/adapter/reference.
