# Template PATCH partial update

Thay placeholder `<...>` theo owner service. Chọn đúng một patch format; không
trộn JSON Merge Patch, JSON Patch và typed partial request.

## Phiếu chốt contract

```text
Owner service:
Operation: PATCH /api/<resources>/{<resourceId>}
Patch format:
Content-Type:
Allowed fields:
Read-only/disallowed fields:
Absent semantics:
Explicit null semantics theo từng field:
Empty patch/no-op behavior:
Authentication/authorization/ownership:
Concurrency token và stale behavior:
Success: [ ] 200 + ApiResponse<T>  [ ] 204
Business preconditions:
Side effects và cách chống lặp:
Error codes (gồm malformed/disallowed/415/concurrency):
API doc:
Business-flow 1:1:
Postman request:
```

## Bảng field presence

| Field | Allowed | Required khi present | Absent | `null` | Validation | Side effect |
| --- | --- | --- | --- | --- | --- | --- |
| `<name>` | Có | Có | Giữ nguyên | Lỗi | `<length>` | `<...>` |
| `<description>` | Có | Không | Giữ nguyên | Clear | `<length>` | Không |
| `<status>` | Có | Có | Giữ nguyên | Lỗi | `<enum/transition>` | `<event>` |
| `<id>` | Không | - | - | - | Reject | - |

## API document

````markdown
# Cập nhật một phần <tài nguyên>

`PATCH /api/<resources>/{<resourceId>}`

## Mục đích
<Kết quả nghiệp vụ, owner service và phạm vi partial update.>

## Xác thực và phân quyền
- Xác thực: <...>
- Vai trò/phạm vi và ownership: <...>

## Yêu cầu
- Content-Type: `<application/merge-patch+json | application/json-patch+json | application/json>`.
- `<resourceId>`: <type/validation>.
- `If-Match`: <format/yêu cầu>, nếu dùng.

```json
{
  "description": null,
  "status": "PUBLISHED"
}
```

- Allowed fields: `<...>`.
- Field absent giữ nguyên.
- Explicit `null`: <policy từng field>.
- Unknown/read-only field: validation error.
- Empty/no-op patch: <behavior>.

## Phản hồi thành công
<Chọn duy nhất 200 với standard envelope hoặc 204 body rỗng.>

## Mã trạng thái HTTP
- `400 <VALIDATION_CODE>`: malformed/empty/disallowed/invalid null/value.
- `401 UNAUTHENTICATED`: <...>
- `403 <ACCESS_CODE>`: <...>
- `404 <RESOURCE_NOT_FOUND>`: <...>
- `409 <CONFLICT_CODE>`: stale token hoặc business conflict.
- `415 UNSUPPORTED_MEDIA_TYPE`: Content-Type không được hỗ trợ.

## Điều kiện nghiệp vụ, concurrency và tác động phụ
- <Cross-field/state transition rules>.
- <Conditional update/version mới>.
- <Database/event/dependency side effects và no-op behavior>.
````

## Business-flow 1:1

```markdown
# Cập nhật một phần <tài nguyên>

## Tác nhân và mục tiêu
- Tác nhân: <...>
- Endpoint duy nhất: `PATCH /api/<resources>/{<resourceId>}`.

## Điều kiện đầu vào
- <Quyền, state, concurrency token, patch media type>.

## Luồng chính
1. API parse patch và reject field/path/operation ngoài allowlist.
2. API bảo toàn absent/null/value khi map sang command.
3. Application tải aggregate và kiểm tra quyền, state, version.
4. Application validate state cuối và chỉ áp dụng field present.
5. Infrastructure conditional update và commit transaction.
6. Side effect chỉ chạy cho state transition thực tế.
7. API trả <200 representation hoặc 204>.

## Trường hợp lỗi
- <malformed/empty/disallowed/null/access/not-found/conflict/415>.

## Dữ liệu thay đổi
- Field present: <...>.
- Field absent giữ nguyên; no-op: <version/audit/event behavior>.
```

## Code skeleton cho typed partial request

### Presence type

```csharp
public readonly record struct PatchField<T>(
    bool IsPresent,
    T? Value)
{
    public static PatchField<T> Absent() => new(false, default);
    public static PatchField<T> Present(T? value) => new(true, value);
}

public sealed record PatchResourceInput(
    PatchField<string> Name,
    PatchField<string?> Description,
    PatchField<string> Status);
```

Api cần custom `JsonConverter` hoặc parse bằng `JsonDocument` để tạo
`PatchField<T>` chính xác. Không để JSON concern này đi vào Application.

### Command và validator

```csharp
public sealed record PatchResourceCommand(
    Guid ResourceId,
    PatchField<string> Name,
    PatchField<string?> Description,
    PatchField<string> Status,
    long ExpectedVersion);

public static class PatchResourceValidator
{
    public static void Validate(PatchResourceCommand command)
    {
        if (!command.Name.IsPresent &&
            !command.Description.IsPresent &&
            !command.Status.IsPresent)
        {
            throw ResourceErrors.EmptyPatch();
        }

        if (command.Name.IsPresent &&
            string.IsNullOrWhiteSpace(command.Name.Value))
        {
            throw ResourceErrors.InvalidField("name");
        }

        if (command.Status.IsPresent && command.Status.Value is null)
        {
            throw ResourceErrors.InvalidField("status");
        }
    }
}
```

Thêm validation length/enum/cross-field theo state cuối, không chỉ theo từng
property độc lập.

### Handler

```csharp
public sealed class PatchResourceHandler(IResourceRepository repository)
{
    public async Task<PatchResourceResult> HandleAsync(
        PatchResourceCommand command,
        CancellationToken cancellationToken)
    {
        PatchResourceValidator.Validate(command);
        var resource = await repository.GetByIdAsync(
            command.ResourceId,
            cancellationToken)
            ?? throw ResourceErrors.NotFound();

        ResourcePolicy.EnsureCanPatch(resource, command);
        var changeSet = ResourcePatch.Apply(resource, command);
        if (!changeSet.HasChanges)
        {
            return PatchResourceResult.From(resource);
        }

        var outcome = await repository.UpdateAsync(
            changeSet,
            command.ExpectedVersion,
            cancellationToken);
        return outcome switch
        {
            ConditionalUpdateResult.Updated => changeSet.ToResult(),
            ConditionalUpdateResult.NotFound => throw ResourceErrors.NotFound(),
            ConditionalUpdateResult.ConcurrencyConflict =>
                throw ResourceErrors.ConcurrencyConflict(),
            _ => throw new InvalidOperationException("Unknown update result.")
        };
    }
}
```

### Parser allowlist

```csharp
private static readonly HashSet<string> AllowedProperties =
    new(StringComparer.Ordinal)
    {
        "name",
        "description",
        "status"
    };

foreach (var property in document.RootElement.EnumerateObject())
{
    if (!AllowedProperties.Contains(property.Name))
    {
        throw ResourceErrors.FieldNotAllowed(property.Name);
    }
}
```

Parser còn phải reject duplicate property nếu policy yêu cầu, sai JSON type và
malformed document; không mass-assign vào entity.

### Minimal API

```csharp
public static RouteHandlerBuilder MapPatchResource(
    this IEndpointRouteBuilder endpoints) =>
    endpoints
        .MapPatch(
            ApiRoutes.Resources.ByIdTemplate,
            async (
                string resourceId,
                HttpRequest request,
                HttpContext context,
                PatchResourceHandler handler,
                CancellationToken cancellationToken) =>
            {
                var input = await PatchResourceParser.ParseAsync(
                    request,
                    cancellationToken);
                var command = PatchResourceMapper.ToCommand(
                    resourceId,
                    input,
                    request.Headers.IfMatch);
                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);
                return Results.Json(
                    ApiResponseFactory.Success(result, context.TraceIdentifier));
            })
        .WithName("patch-resource")
        .WithTags(ServiceNames.Course)
        .Accepts<object>("application/merge-patch+json")
        .Produces<ApiResponse<PatchResourceResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .Produces<ApiErrorResponse>(StatusCodes.Status415UnsupportedMediaType);
```

Thay `Course`, route, media type, response và status bằng contract thật. Nếu chọn
`204`, trả `Results.NoContent()` và khai báo metadata tương ứng.

## Ma trận test

- [ ] Unit: từng field absent giữ nguyên.
- [ ] Unit: explicit null clear field nullable; reject field non-nullable.
- [ ] Unit: value hợp lệ/không hợp lệ và cross-field final-state rule.
- [ ] Unit: empty patch/no-op theo đúng contract.
- [ ] Unit: unknown/read-only field và operation/path bị cấm.
- [ ] Unit: current/stale concurrency token.
- [ ] Component: đúng/sai Content-Type, malformed JSON và binding.
- [ ] Component: success contract, error envelope, auth và metadata.
- [ ] Integration: database chỉ đổi subset present.
- [ ] Integration: absent fields giữ nguyên, explicit null persist thành `NULL`.
- [ ] Integration: rollback và hai writer cạnh tranh.

## Postman và quality gate

- [ ] Request có đúng Content-Type, auth, route variable, concurrency header.
- [ ] Examples: one-field value, explicit null, field absent.
- [ ] Error cases: empty/malformed, unknown/read-only, invalid null, `404`, stale.
- [ ] Tests kiểm tra status, envelope/body và version mới.
- [ ] API doc và business-flow 1:1 đồng bộ.
- [ ] Feature/integration tests, `dotnet build`, `dotnet test` pass.
- [ ] `dotnet format --verify-no-changes` pass.
