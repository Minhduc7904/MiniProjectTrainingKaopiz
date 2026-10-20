# Template PUT full replacement

Thay toàn bộ placeholder `<...>` và điều chỉnh theo convention của owner service.
Không copy skeleton nếu abstraction tương đương đã tồn tại.

## Phiếu chốt contract

```text
Owner service:
Operation: PUT /api/<resources>/{<resourceId>}
Mục tiêu full replacement:
Client-managed fields (đầy đủ):
Server-managed fields (không nhận):
Authentication/authorization/ownership:
Success: [ ] 200 + ApiResponse<T>  [ ] 204, body rỗng
Concurrency: [ ] If-Match/ETag  [ ] version field
Token bắt buộc/sai/stale:
Idempotency khi gửi lại cùng request:
Business preconditions:
Side effects và cách chống lặp:
Error codes:
API doc:
Business-flow 1:1:
Postman folder/request:
```

## API document

````markdown
# Thay thế toàn bộ <tài nguyên>

`PUT /api/<resources>/{<resourceId>}`

## Mục đích
<Kết quả nghiệp vụ và owner service. Đây là full replacement.>

## Xác thực và phân quyền
- Xác thực: <...>
- Vai trò/phạm vi: <...>
- Quy tắc sở hữu: <...>

## Yêu cầu
- `<resourceId>`: <type, validation>.
- `If-Match`: <bắt buộc, format/version>, nếu dùng.

```json
{
  "name": "Backend Fundamentals",
  "description": null,
  "status": "DRAFT",
  "version": 3
}
```

Mọi client-managed field phải xuất hiện theo contract; field thiếu không được
hiểu là giữ nguyên. <Liệt kê type, required, validation và ý nghĩa từng field.>

## Phản hồi thành công
<Chọn duy nhất 200 với envelope và ví dụ cụ thể, hoặc 204 No Content body rỗng.>

## Mã trạng thái HTTP
- `400 <VALIDATION_CODE>`: <...>
- `401 UNAUTHENTICATED`: <...>
- `403 <ACCESS_CODE>`: <...>
- `404 <RESOURCE_NOT_FOUND>`: <...>
- `409 <RESOURCE_CONCURRENCY_CONFLICT>`: version không còn hiện hành.

## Điều kiện nghiệp vụ, concurrency và tác động phụ
- Replacement của collection/null/default: <...>
- Cùng request + version hiện hành: <hành vi idempotent>.
- Conditional update/version mới: <...>
- Bản ghi/event/dependency thay đổi: <...>
````

## Business-flow 1:1

```markdown
# Thay thế toàn bộ <tài nguyên>

## Tác nhân và mục tiêu
- Tác nhân: <...>
- Mục tiêu: <...>
- Endpoint duy nhất: `PUT /api/<resources>/{<resourceId>}`.

## Điều kiện đầu vào
- <Quyền sở hữu, trạng thái, version và full payload.>

## Luồng chính
1. API validate ID, header/token và toàn bộ payload.
2. Application tải aggregate và kiểm tra quyền/preconditions/version.
3. Application thay thế toàn bộ client-managed state.
4. Infrastructure conditional update và commit transaction.
5. Hệ thống thực hiện side effect đúng một lần cho state transition.
6. API trả <200 representation hoặc 204>.

## Trường hợp lỗi
- <validation/not-found/access/concurrency/business conflict>.

## Dữ liệu thay đổi và tính lũy đẳng
- <table/fields/version/audit/event/outbox>.
- Gửi lại cùng request không tạo thêm thay đổi hoặc side effect.
```

## Code skeleton

### Request, command và result

```csharp
public sealed record ReplaceResourceRequest(
    string Name,
    string? Description,
    string Status,
    long Version);

public sealed record ReplaceResourceCommand(
    Guid ResourceId,
    string Name,
    string? Description,
    string Status,
    long ExpectedVersion);

public sealed record ReplaceResourceResult(
    Guid ResourceId,
    string Name,
    string? Description,
    string Status,
    long Version);
```

Không dùng nullable cho required field chỉ để biến field vắng thành “không đổi”.
Validator phải kiểm tra required, range, enum, cross-field và version.

### Persistence abstraction

```csharp
public interface IResourceRepository
{
    Task<ResourceSnapshot?> GetByIdAsync(
        Guid resourceId,
        CancellationToken cancellationToken);

    Task<ConditionalUpdateResult> ReplaceAsync(
        ResourceReplacement replacement,
        long expectedVersion,
        CancellationToken cancellationToken);
}

public enum ConditionalUpdateResult
{
    Updated,
    NotFound,
    ConcurrencyConflict
}
```

### Handler

```csharp
public sealed class ReplaceResourceHandler(IResourceRepository repository)
{
    public async Task<ReplaceResourceResult> HandleAsync(
        ReplaceResourceCommand command,
        CancellationToken cancellationToken)
    {
        ReplaceResourceValidator.Validate(command);

        var current = await repository.GetByIdAsync(
            command.ResourceId,
            cancellationToken);
        if (current is null)
        {
            throw ResourceErrors.NotFound();
        }

        ResourcePolicy.EnsureCanReplace(current, command);
        var replacement = ResourceReplacement.From(command);
        var outcome = await repository.ReplaceAsync(
            replacement,
            command.ExpectedVersion,
            cancellationToken);

        return outcome switch
        {
            ConditionalUpdateResult.Updated =>
                replacement.ToResult(command.ExpectedVersion + 1),
            ConditionalUpdateResult.NotFound =>
                throw ResourceErrors.NotFound(),
            ConditionalUpdateResult.ConcurrencyConflict =>
                throw ResourceErrors.ConcurrencyConflict(),
            _ => throw new InvalidOperationException("Unknown update result.")
        };
    }
}
```

Production code cần transaction/outbox và state-change detection theo side effect
thực tế; skeleton không thay cho business policy.

### Minimal API

```csharp
public static RouteHandlerBuilder MapReplaceResource(
    this IEndpointRouteBuilder endpoints) =>
    endpoints
        .MapPut(
            ApiRoutes.Resources.ByIdTemplate,
            async (
                string resourceId,
                ReplaceResourceRequest request,
                HttpContext context,
                ReplaceResourceHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = ReplaceResourceMapper.ToCommand(
                    resourceId,
                    request,
                    context.Request.Headers.IfMatch);
                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                return Results.Json(
                    ApiResponseFactory.Success(
                        ReplaceResourceMapper.ToResponse(result),
                        context.TraceIdentifier));
                // Nếu contract là 204, thay return trên bằng Results.NoContent().
            })
        .WithName("replace-resource")
        .WithTags(ServiceNames.Course)
        .Accepts<ReplaceResourceRequest>("application/json")
        .Produces<ApiResponse<ReplaceResourceResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);
```

Xóa metadata `200` và response type nếu contract chọn `204`; thêm
`.Produces(StatusCodes.Status204NoContent)`. Bổ sung `401/403/5xx` đúng hành vi.

## Ma trận test

- [ ] Unit: ID/token/payload không hợp lệ.
- [ ] Unit: mọi field và collection được thay thế, không merge ngầm.
- [ ] Unit: not-found, access denied, business precondition.
- [ ] Unit: current version thành công; stale version conflict.
- [ ] Unit: gọi lặp không nhân đôi side effect.
- [ ] Component: route, JSON binding, `If-Match`, endpoint metadata.
- [ ] Component: đúng một success contract (`200` envelope hoặc `204` body rỗng).
- [ ] Component: mọi expected error dùng safe envelope và `traceId`.
- [ ] Integration: conditional update và version increment trên database thật.
- [ ] Integration: rollback không để partial replacement.
- [ ] Integration: hai writer dùng cùng version, chỉ một writer thành công.

## Postman và quality gate

- [ ] Request dùng route variable, auth và `Content-Type: application/json`.
- [ ] Full payload chứa mọi client-managed field.
- [ ] `If-Match`/version lấy từ environment hoặc pre-request script.
- [ ] Tests kiểm tra success contract, body/envelope và version mới.
- [ ] Có examples/cases validation, `404` và concurrency conflict.
- [ ] API doc và business-flow 1:1 đã đồng bộ.
- [ ] Feature tests và integration tests pass.
- [ ] `dotnet build` pass.
- [ ] `dotnet test` pass.
- [ ] `dotnet format --verify-no-changes` pass.
