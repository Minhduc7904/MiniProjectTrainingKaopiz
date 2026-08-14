# Template POST endpoint

Thay placeholder `<...>`. Chọn **create `201`** hoặc **action `202`** và xóa
nhánh không áp dụng khỏi contract/implementation.

## Phiếu chốt contract

```text
Owner service:
Loại: create đồng bộ | action bất đồng bộ
Direct/public POST path:
Canonical resource GET hoặc operation-status GET:
Content type:
Authentication/ownership:
Request fields + validation:
Business preconditions:
Success: 201 | 202
Location:
Idempotency key source/required:
Key scope + retention:
Payload fingerprint:
Replay/concurrent behavior:
Same key, different payload error:
Transaction/outbox/side effects:
Compensation:
```

## API doc

Tạo đúng một file
`docs/api/<owning-service>/endpoints/post-<resource-or-action>.md`:

````markdown
# `POST /<gateway-prefix>/api/<resource-or-action>`

## Mục đích
<Create đồng bộ hoặc action bất đồng bộ và owner>. Direct path là `<...>`.

## Xác thực và phân quyền
- Xác thực: <...>.
- Ownership/role: <...>.

## Yêu cầu

`Content-Type: application/json`
`Idempotency-Key: <opaque-client-key>`

```json
{
  "<field>": "<value>"
}
```

| Field/header | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `Idempotency-Key` | string | Có | <length/scope>. |
| `<field>` | ... | ... | ... |

## Phản hồi thành công

`<201 Created | 202 Accepted>`

```http
Location: /<gateway-prefix>/api/<resources-or-operations>/<id>
```

```json
{
  "data": {
    "id": "11111111-1111-1111-1111-111111111111"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Idempotency và xử lý đồng thời
- Scope/fingerprint/TTL: <...>.
- Same key + same payload: <replay cùng logical result>.
- Same key + different payload: `409 IDEMPOTENCY_KEY_REUSED`.
- Concurrent duplicate: <...>.

## Mã trạng thái HTTP
- `<201|202>`: <...>.
- `400 VALIDATION_FAILED`: <...>.
- `409 IDEMPOTENCY_KEY_REUSED`: <...>.
- <Auth/not-found/business/dependency errors thực tế>.

## Điều kiện nghiệp vụ và tác động phụ
- Preconditions: <...>.
- Transaction/outbox: <...>.
- Rows/messages/storage/HTTP side effects: <...>.
- Retry/compensation: <...>.
````

## Business flow 1:1

Tạo đúng một file
`docs/business-flows/<domain>/post-<resource-or-action>.md`:

```markdown
# <Tạo resource | Bắt đầu action>

## Mục đích và tác nhân
<...>

## Điều kiện đầu vào
- <Auth/ownership>.
- <Business preconditions>.
- Idempotency key <...>.

## Luồng chính
1. Client gửi POST với payload và idempotency key.
2. API validate syntax; Application validate invariant.
3. Service atomically claim key và fingerprint.
4. <Create: ghi resource | Action: ghi operation + outbox>.
5. Commit transaction.
6. API trả `<201|202>` và `Location`.

## Replay và xử lý đồng thời
- Same key/same payload: <...>.
- Same key/different payload: <...>.
- Hai request đồng thời: <...>.

## Trường hợp lỗi và compensation
<...>

## Dữ liệu thay đổi
- Database: <...>.
- Message/job/storage/dependency: <...>.
```

## Shared route/header skeleton

```csharp
public static class ApiHeaderNames
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string IdempotencyKey = "Idempotency-Key";
}

public static class ApiRoutes
{
    public static class <Resources>
    {
        public const string Create = "/api/<resources>";
        public const string GetByIdTemplate = "/api/<resources>/{<resourceId>}";

        public static string GetByIdPublicPath(Guid id) =>
            BuildPublicPath(
                GatewayRoutePrefixes.<Service>,
                FormatGuidRoute(GetByIdTemplate, "<resourceId>", id));
    }
}
```

Với action `202`, thay canonical resource path bằng
`/api/<operations>/{operationId}`.

## Application create skeleton

```csharp
public sealed record Create<Resource>Command(
    string IdempotencyKey,
    string Name,
    Guid ActorId);

public sealed record Create<Resource>Result(Guid Id, string Name);

public sealed class Create<Resource>Handler(
    I<Resource>Repository repository,
    IIdempotencyStore idempotencyStore,
    IUnitOfWork unitOfWork)
{
    public async Task<Create<Resource>Result> HandleAsync(
        Create<Resource>Command command,
        CancellationToken cancellationToken)
    {
        Validate(command);
        var fingerprint = <Fingerprint>.Create(command);
        var claim = await idempotencyStore.ClaimAsync(
            command.IdempotencyKey,
            fingerprint,
            cancellationToken);

        if (claim.IsReplay)
        {
            return claim.GetResult<Create<Resource>Result>();
        }

        var resource = <Resource>.Create(Guid.NewGuid(), command.Name);
        await repository.AddAsync(resource, cancellationToken);
        var result = new Create<Resource>Result(resource.Id, resource.Name);
        await idempotencyStore.CompleteAsync(claim, result, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
```

Abstraction phải bảo đảm claim/resource/result atomic trong adapter thực tế.
Skeleton không thay thế transaction/concurrency implementation.

## Application async action skeleton

```csharp
public sealed record Start<Action>Command(
    string IdempotencyKey,
    Guid TargetId,
    Guid ActorId);

public sealed record Start<Action>Result(
    Guid OperationId,
    string Status);

public sealed class Start<Action>Handler(
    IOperationRepository operations,
    IOutboxWriter outbox,
    IIdempotencyStore idempotencyStore,
    IUnitOfWork unitOfWork)
{
    public async Task<Start<Action>Result> HandleAsync(
        Start<Action>Command command,
        CancellationToken cancellationToken)
    {
        Validate(command);
        var fingerprint = <Fingerprint>.Create(command);
        var claim = await idempotencyStore.ClaimAsync(
            command.IdempotencyKey,
            fingerprint,
            cancellationToken);
        if (claim.IsReplay)
        {
            return claim.GetResult<Start<Action>Result>();
        }

        var operation = <Operation>.Pending(
            Guid.NewGuid(),
            command.TargetId,
            command.ActorId);
        await operations.AddAsync(operation, cancellationToken);
        await outbox.AddAsync(
            new <Action>Requested(operation.Id, command.TargetId),
            cancellationToken);
        var result = new Start<Action>Result(operation.Id, "PENDING");
        await idempotencyStore.CompleteAsync(claim, result, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
```

## API create `201` skeleton

```csharp
public static RouteHandlerBuilder MapCreate<Resource>(
    this IEndpointRouteBuilder endpoints) =>
    endpoints
        .MapPost(
            ApiRoutes.<Resources>.Create,
            async (
                Create<Resource>Request request,
                HttpContext context,
                Create<Resource>Handler handler,
                CancellationToken cancellationToken) =>
            {
                var key = RequireIdempotencyKey(context.Request.Headers);
                var result = await handler.HandleAsync(
                    new Create<Resource>Command(
                        key,
                        request.Name,
                        <Actor>.From(context)),
                    cancellationToken);
                context.Response.Headers.Location =
                    ApiRoutes.<Resources>.GetByIdPublicPath(result.Id);

                return Results.Json(
                    ApiResponseFactory.Success(
                        result,
                        context.TraceIdentifier),
                    statusCode: StatusCodes.Status201Created);
            })
        .WithName("create-<resource>")
        .WithTags(ServiceNames.<Service>)
        .Accepts<Create<Resource>Request>("application/json")
        .Produces<ApiResponse<Create<Resource>Result>>(
            StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);
```

## API action `202` skeleton

```csharp
var result = await handler.HandleAsync(command, cancellationToken);
context.Response.Headers.Location =
    ApiRoutes.<Operations>.GetByIdPublicPath(result.OperationId);

return Results.Json(
    ApiResponseFactory.Success(result, context.TraceIdentifier),
    statusCode: StatusCodes.Status202Accepted);
```

Khai báo `.Produces<ApiResponse<Start<Action>Result>>(202)` và errors thực tế.
Đăng ký endpoint trong `Program.cs`.

## Infrastructure idempotency skeleton

```csharp
// Unique index: (scope, idempotency_key)
public async Task<IdempotencyClaim> ClaimAsync(
    string key,
    string fingerprint,
    CancellationToken cancellationToken)
{
    // Insert claim trong transaction.
    // Duplicate key: load existing row.
    // Fingerprint khác: throw IDEMPOTENCY_KEY_REUSED.
    // Completed: deserialize safe stored result để replay.
    // Processing: trả operation hiện tại hoặc behavior đã document.
}
```

Không nuốt duplicate exception; chỉ map error number/provider-specific tại
Infrastructure. Không serialize secret/credential vào stored response.

## Test skeleton

```csharp
[Test]
public async Task SameKeyAndPayloadReplaySameLogicalResult()
{
    var first = await handler.HandleAsync(command, CancellationToken.None);
    var replay = await handler.HandleAsync(command, CancellationToken.None);

    Assert.Multiple(() =>
    {
        Assert.That(replay.Id, Is.EqualTo(first.Id));
        Assert.That(repository.CreatedCount, Is.EqualTo(1));
        Assert.That(outbox.MessageCount, Is.LessThanOrEqualTo(1));
    });
}

[Test]
public async Task PostReturnsSuccessAndCanonicalLocation()
{
    using var request = CreateHttpRequest(idempotencyKey, payload);
    using var response = await client.SendAsync(request);

    Assert.Multiple(() =>
    {
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.<CreatedOrAccepted>));
        Assert.That(
            response.Headers.Location?.OriginalString,
            Is.EqualTo(expectedPublicLocation));
    });
}
```

Integration test phải chạy concurrent requests cùng key, xác nhận một
resource/operation/outbox row; rollback phải không để claim completed giả.

## Postman skeleton

```json
{
  "name": "<Create resource | Start action>",
  "request": {
    "method": "POST",
    "header": [
      {
        "key": "Content-Type",
        "value": "application/json"
      },
      {
        "key": "Idempotency-Key",
        "value": "{{idempotencyKey}}"
      }
    ],
    "body": {
      "mode": "raw",
      "raw": "{\"name\":\"example\"}"
    },
    "url": "{{<service>BaseUrl}}/api/<resource-or-action>"
  }
}
```

Pre-request script có thể tạo key mới khi collection variable rỗng. Test script
kiểm tra exact status, envelope, `Location`, lưu ID/Location; request replay dùng
cùng key/payload.

## Checklist bàn giao

- [ ] Đã đọc reference/template và ba test skill.
- [ ] Chỉ POST; đã chọn create `201` hoặc action `202`.
- [ ] `Location` trỏ đúng canonical resource/operation status path.
- [ ] Idempotency scope/fingerprint/replay/conflict/retention đã chốt.
- [ ] Transaction, outbox và compensation đúng side effects.
- [ ] Error envelope an toàn.
- [ ] Unit/component/integration bao phủ replay và race.
- [ ] Đúng một API doc và một business-flow file 1:1.
- [ ] Postman JSON hợp lệ và có replay case.
- [ ] Build, full test và format verify đều đạt.
