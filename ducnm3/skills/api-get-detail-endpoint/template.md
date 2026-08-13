# Template GET detail endpoint

Thay toàn bộ placeholder `<...>`; bỏ section không áp dụng nhưng không bỏ qua
quyết định contract tương ứng.

## Phiếu chốt contract

```text
Owner service:
Resource/ID:
Direct path: GET /api/<resources>/{<resourceId>}
Public path: GET /<gateway-prefix>/api/<resources>/{<resourceId>}
Authentication:
Authorization/visibility:
200 representation:
400 conditions:
404 code:
Dependency errors:
Cache-Control:
ETag source/304 behavior (hoặc "không dùng"):
Side effects: không có
```

## API doc

Tạo đúng một file
`docs/api/<owning-service>/endpoints/get-<resource>-by-id.md`:

````markdown
# `GET /<gateway-prefix>/api/<resources>/{<resourceId>}`

## Mục đích

<Kết quả và owner service>. Direct service path là
`GET /api/<resources>/{<resourceId>}`.

## Xác thực và phân quyền

- Xác thực: <...>.
- Phân quyền/ownership/visibility: <...>.

## Yêu cầu

| Tham số | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `<resourceId>` | UUID | Có | UUID hợp lệ, khác rỗng. |

Không có request body. <Nêu query/header nếu có>.

## Phản hồi thành công

`200 OK`

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

Cache: `<Cache-Control policy>`. <ETag/If-None-Match và 304 nếu có>.

## Mã trạng thái HTTP

- `200`: <...>.
- `400 VALIDATION_FAILED`: <...>.
- `404 <RESOURCE>_NOT_FOUND`: <...>.
- <Auth/dependency/5xx thực tế>.

Lỗi dùng [error envelope chuẩn](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

- Request safe, idempotent và chỉ đọc.
- <Bảng/dependency được đọc>.
- Không thay đổi database, publish message hoặc tạo job.
````

## Business flow 1:1

Tạo đúng một file
`docs/business-flows/<domain>/get-<resource>-by-id.md`:

```markdown
# Lấy chi tiết <resource>

## Mục đích
<...>

## Tác nhân
<Client, Gateway, owner service, dependency>.

## Điều kiện đầu vào
- `<resourceId>` hợp lệ.
- <Quyền/visibility>.

## Luồng chính
1. Client gửi GET không body.
2. API validate route và identity.
3. Application kiểm tra quyền, gọi read abstraction.
4. Infrastructure đọc projection.
5. API trả `200` envelope và cache header.

## Cache
<no-store/private/public/ETag và 304>.

## Trường hợp lỗi
- `400`: <...>.
- `404`: <...>.
- <...>.

## Dữ liệu thay đổi
Không có; endpoint chỉ đọc <bảng/dependency>.
```

## Route skeleton

```csharp
public static class ApiRoutes
{
    public static class <Resources>
    {
        public const string GetByIdTemplate =
            "/api/<resources>/{<resourceId>}";

        public static string GetByIdServicePath(Guid id) =>
            BuildServicePath(
                FormatGuidRoute(GetByIdTemplate, "<resourceId>", id));

        public static string GetByIdPublicPath(Guid id) =>
            BuildPublicPath(
                GatewayRoutePrefixes.<Service>,
                FormatGuidRoute(GetByIdTemplate, "<resourceId>", id));
    }
}
```

Thêm test cho template, service path, public path và `Guid.Empty`.

## Application skeleton

```csharp
public sealed record Get<Resource>ByIdQuery(Guid Id);

public sealed record Get<Resource>ByIdResult(
    Guid Id,
    string Name,
    DateTime UpdatedAtUtc);

public interface I<Resource>QueryRepository
{
    Task<Get<Resource>ByIdResult?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}

public sealed class Get<Resource>ByIdHandler(
    I<Resource>QueryRepository repository)
{
    public async Task<Get<Resource>ByIdResult> HandleAsync(
        Get<Resource>ByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Id == Guid.Empty)
        {
            throw <Resource>Errors.Validation("<resourceId>", "Invalid UUID.");
        }

        return await repository.GetByIdAsync(
            query.Id,
            cancellationToken) ?? throw <Resource>Errors.NotFound();
    }
}
```

Đưa authorization/visibility rule vào handler hoặc Application service, không
đặt trong EF adapter hay endpoint.

## Infrastructure skeleton

```csharp
public sealed class Ef<Resource>QueryRepository(<Service>DbContext dbContext)
    : I<Resource>QueryRepository
{
    public Task<Get<Resource>ByIdResult?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.<Resources>
            .AsNoTracking()
            .Where(item =>
                item.Id == id &&
                item.DeletedAt == null)
            .Select(item => new Get<Resource>ByIdResult(
                item.Id,
                item.Name,
                item.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
}
```

Đăng ký interface/adapter trong Infrastructure và handler trong Application.

## API endpoint skeleton

```csharp
public static RouteHandlerBuilder MapGet<Resource>ById(
    this IEndpointRouteBuilder endpoints) =>
    endpoints
        .MapGet(
            ApiRoutes.<Resources>.GetByIdTemplate,
            async (
                string <resourceId>,
                HttpContext context,
                Get<Resource>ByIdHandler handler,
                CancellationToken cancellationToken) =>
            {
                if (!Guid.TryParse(<resourceId>, out var id) ||
                    id == Guid.Empty)
                {
                    throw <Resource>Errors.Validation(
                        "<resourceId>",
                        "Invalid UUID.");
                }

                var result = await handler.HandleAsync(
                    new Get<Resource>ByIdQuery(id),
                    cancellationToken);
                context.Response.Headers.CacheControl = "no-store";

                return Results.Json(
                    ApiResponseFactory.Success(
                        result,
                        context.TraceIdentifier));
            })
        .WithName("get-<resource>-by-id")
        .WithTags(ServiceNames.<Service>)
        .Produces<ApiResponse<Get<Resource>ByIdResult>>(
            StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
```

Nếu dùng ETag, tạo ETag từ version ổn định, kiểm tra `If-None-Match` trước khi
serialize và trả `Results.StatusCode(StatusCodes.Status304NotModified)` không
body. Không copy `no-store` nếu contract đã chọn policy khác.

## Test skeleton

```csharp
[Test]
public async Task ExistingResourceReturnsEnvelopeAndCachePolicy()
{
    using var response = await client.GetAsync(
        ApiRoutes.<Resources>.GetByIdServicePath(existingId));
    var envelope = await response.Content.ReadFromJsonAsync<
        ApiResponse<Get<Resource>ByIdResult>>();

    Assert.Multiple(() =>
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(envelope!.Data.Id, Is.EqualTo(existingId));
        Assert.That(envelope.Meta.TraceId, Is.Not.Empty);
        Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
    });
}

[Test]
public async Task MissingResourceReturnsSafeNotFoundEnvelope()
{
    using var response = await client.GetAsync(
        ApiRoutes.<Resources>.GetByIdServicePath(Guid.NewGuid()));
    var body = await response.Content.ReadAsStringAsync();

    Assert.Multiple(() =>
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(body, Does.Contain("<RESOURCE>_NOT_FOUND"));
        Assert.That(body, Does.Not.Contain("stack"));
    });
}
```

Integration test phải seed row visible/hidden/deleted, gọi adapter hoặc API thật,
xác nhận projection và xác nhận row/version/audit không đổi sau GET.

## Postman request skeleton

```json
{
  "name": "Get <resource> by id",
  "request": {
    "method": "GET",
    "header": [],
    "url": "{{<service>BaseUrl}}/api/<resources>/{{<resource>Id}}"
  }
}
```

Thêm `event` test script cho `200`, envelope, ID, cache header; thêm case `404`
và conditional request nếu có ETag. Không thêm thuộc tính `body`.

## Checklist bàn giao

- [ ] Đã đọc `reference.md`, `template.md` và các test skill bắt buộc.
- [ ] Chỉ triển khai GET detail; không body, safe, idempotent.
- [ ] Route/constants không có magic string lặp.
- [ ] Application/Infrastructure/API đúng dependency direction.
- [ ] `200`/`404` và error envelope đúng.
- [ ] Cache policy và ETag/`304` nếu có đã test.
- [ ] Có unit, component và integration coverage.
- [ ] Có đúng một API doc và một business-flow file 1:1.
- [ ] Postman collection đã cập nhật và JSON hợp lệ.
- [ ] Feature tests, integration tests, build, full tests và format đều đạt.
