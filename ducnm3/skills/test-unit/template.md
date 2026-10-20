---
title: Template unit test
description: Fixture NUnit, reusable test double và checklist triển khai unit test.
---

# Template unit test

## Phiếu chốt phạm vi

Điền trước khi viết code:

```text
Behavior:
SUT:
Production owner:
Exact test project:
Exact test file:
Catalog: docs/tests/<owner>/unit.md
Success cases:
Error/boundary cases:
Nguồn không xác định cần kiểm soát:
Reusable test doubles:
Focused test command:
```

Nếu có `TestServer`, connection string thật, container hoặc network endpoint,
dừng và chọn skill khác.

## Cấu trúc thư mục

```text
<Owner>.UnitTests/
├── <Area>/
│   └── <Sut>Tests.cs
├── TestDoubles/
│   ├── StubClock.cs
│   └── SpyDependency.cs
└── <Owner>.UnitTests.csproj
```

## Fixture NUnit

```csharp
using NUnit.Framework;

namespace Owner.UnitTests.Area;

public sealed class HandlerTests
{
    private static readonly Guid EntityId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public async Task HandleAsync_EntityExists_ReturnsExpectedResult()
    {
        // Arrange
        var dependency = new StubDependency(new Entity(EntityId, "ACTIVE"));
        var sut = new Handler(dependency);

        // Act
        var result = await sut.HandleAsync(
            EntityId,
            TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(EntityId));
            Assert.That(result.Status, Is.EqualTo("ACTIVE"));
        });
    }

    [Test]
    public void HandleAsync_EntityMissing_ThrowsExpectedBusinessError()
    {
        // Arrange
        var sut = new Handler(new StubDependency(null));

        // Act
        var exception = Assert.ThrowsAsync<ApplicationException>(
            () => sut.HandleAsync(
                EntityId,
                TestContext.CurrentContext.CancellationToken));

        // Assert
        Assert.That(exception!.ErrorCode, Is.EqualTo(ErrorCodes.NotFound));
    }
}
```

Thay type giả bằng type thật của project. Không giữ literal placeholder hoặc
class minh họa trong commit.

## Reusable spy

```csharp
namespace Owner.UnitTests.TestDoubles;

internal sealed class SpyDependency(Result? result) : IDependency
{
    public int CallCount { get; private set; }

    public Guid? LastId { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public Task<Result?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        CallCount++;
        LastId = id;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(result);
    }
}
```

Tạo instance mới cho từng test. Không dùng static mutable state.

## Mẫu catalog

````markdown
## Phạm vi

Dự án: `backend/.../<Owner>.UnitTests/<Owner>.UnitTests.csproj`
Mã nguồn: `<Area>/<Sut>Tests.cs`
Dependency: `<Dependency>` được thay bằng `<Stub/Spy>` trong memory.

Chạy:

```bash
dotnet test backend/.../<Owner>.UnitTests/<Owner>.UnitTests.csproj
```

## Ca kiểm thử

- `<Member>_<Scenario>_<ExpectedResult>`: với `<input/state>`, khi `<action>` thì
  `<observable result/error code>` phải đúng.
````

## Checklist trước bàn giao

### Phân loại

- [ ] Test chỉ kiểm tra Domain/Application/Infrastructure logic deterministic.
- [ ] Không có `TestServer`, Docker, network, database thật hoặc shared resource.
- [ ] Exact `.csproj`, file test và catalog đã được chốt.
- [ ] Không đặt component/integration test vào project unit.

### Thiết kế

- [ ] Mỗi test theo Arrange–Act–Assert và có một behavior rõ ràng.
- [ ] Class dùng `<Sut>Tests`.
- [ ] Method dùng `<Member>_<Scenario>_<ExpectedResult>`.
- [ ] Clock, UUID, random, culture và configuration đã được kiểm soát.
- [ ] Test double dùng lại nằm trong `TestDoubles/`.
- [ ] Success, boundary và expected error phù hợp với contract đã được bao phủ.

### Tài liệu

- [ ] `docs/tests/<owner>/unit.md` liệt kê đúng test đang tồn tại.
- [ ] `docs/tests/README.md` có link nếu catalog mới được tạo.
- [ ] Tài liệu dùng tiếng Việt kỹ thuật và giữ nguyên identifier/contract.

### Verification

- [ ] Focused fixture pass.
- [ ] Exact test project pass.
- [ ] `dotnet format ... --verify-no-changes` pass.
- [ ] Test nhạy với async/cancellation đã được chạy lặp lại.
- [ ] Backend solution test đã chạy khi shared code/reference bị thay đổi.
