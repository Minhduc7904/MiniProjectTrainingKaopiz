# Template DELETE

Thay placeholder `<...>` và xóa nhánh không được chọn. Không để cả soft và hard
delete như hai behavior runtime mơ hồ cho cùng endpoint.

## Phiếu chốt contract

```text
Owner service:
Operation: DELETE /api/<resources>/{<resourceId>}
Deletion policy: [ ] soft delete  [ ] hard delete
Lý do:
Authentication/authorization/ownership:
Concurrency token:
Lần đầu thành công: 204, body rỗng
Resource chưa tồn tại: [ ] 404  [ ] 204 idempotent
Resource đã xóa: [ ] 404  [ ] 204 idempotent
Conflict 409 (dependency/state/concurrency):
Database transaction/cascade/restrict:
Audit/retention/restore/purge:
Events/outbox:
External cleanup và retry/dedupe:
API doc:
Business-flow 1:1:
Postman request:
```

## Checklist policy

### Nếu soft delete

- [ ] Marker gồm status/`deleted_at`/actor/reason/version cần thiết.
- [ ] Default query và repository ẩn record đã xóa.
- [ ] Admin/include-deleted/restore behavior đã chốt.
- [ ] Unique constraint và relation behavior sau khi xóa đã chốt.
- [ ] Retention/purge có owner và lịch rõ ràng.
- [ ] Already-deleted request không phát side effect lặp.

### Nếu hard delete

- [ ] Retention/audit/legal rule cho phép xóa vật lý.
- [ ] Mọi foreign key chọn `CASCADE`, `RESTRICT` hoặc explicit cleanup.
- [ ] Race với dependency mới được chặn bằng transaction/constraint.
- [ ] External data cleanup không làm database ở trạng thái partial.
- [ ] Audit/event cần giữ không phụ thuộc row sắp xóa.

## API document

```markdown
# Xóa <tài nguyên>

`DELETE /api/<resources>/{<resourceId>}`

## Mục đích
<Kết quả nghiệp vụ, owner service và soft/hard delete.>

## Xác thực và phân quyền
- Xác thực: <...>
- Vai trò/phạm vi: <...>
- Quy tắc sở hữu: <...>

## Yêu cầu
- `<resourceId>`: <type/validation>.
- `If-Match`: <format/yêu cầu>, nếu dùng.
- Không có request body.

## Phản hồi thành công
`204 No Content`; response body rỗng.

## Mã trạng thái HTTP
- `400 <VALIDATION_CODE>`: ID/token không hợp lệ.
- `401 UNAUTHENTICATED`: <...>
- `403 <ACCESS_CODE>`: <...>
- `404 <RESOURCE_NOT_FOUND>`: <chưa tồn tại/đã xóa theo contract>.
- `409 <RESOURCE_IN_USE | INVALID_STATE | CONCURRENCY_CONFLICT>`: <...>

## Điều kiện nghiệp vụ, idempotency và tác động phụ
- Deletion policy: <soft/hard và lý do>.
- Gọi lặp: <204 hoặc 404>, không lặp side effect.
- Database: <marker/delete/cascade/restrict/transaction>.
- Event/outbox: <...>.
- External cleanup/retry/dedupe: <...>.
- Retention/restore/purge: <...>.
```

## Business-flow 1:1

```markdown
# Xóa <tài nguyên>

## Tác nhân và mục tiêu
- Tác nhân: <...>
- Endpoint duy nhất: `DELETE /api/<resources>/{<resourceId>}`.

## Điều kiện đầu vào
- <Quyền sở hữu, state, dependency, concurrency token>.

## Luồng chính
1. API validate ID/header và tạo command.
2. Application kiểm tra resource, quyền và deletion policy.
3. Application xử lý already-deleted theo idempotency contract.
4. Infrastructure <mark deleted | hard delete> có điều kiện trong transaction.
5. Outbox/event được ghi cùng transaction.
6. Cleanup consumer xử lý external side effect idempotently.
7. API trả `204 No Content`.

## Trường hợp lỗi
- `404`: <...>.
- `409`: <dependency/state/concurrency>.
- <access/validation/dependency failure>.

## Dữ liệu và side effects
- Database: <row/fields/relations/outbox>.
- Visibility/retention/purge: <...>.
- External cleanup, retry và dedupe key: <...>.
```

## Code skeleton

### Command và outcome

```csharp
public sealed record DeleteResourceCommand(
    Guid ResourceId,
    Guid ActorId,
    long? ExpectedVersion);

public enum DeleteResourceOutcome
{
    Deleted,
    NotFound,
    AlreadyDeleted,
    Conflict,
    ConcurrencyConflict
}

public interface IResourceDeletionRepository
{
    Task<DeleteResourceOutcome> DeleteAsync(
        DeleteResourceCommand command,
        CancellationToken cancellationToken);
}
```

Đổi tên `DeleteAsync` thành `SoftDeleteAsync`/`HardDeleteAsync` nếu giúp policy
rõ hơn. Adapter không trả raw database exception cho Application/API.

### Handler

```csharp
public sealed class DeleteResourceHandler(
    IResourceDeletionRepository repository)
{
    public async Task HandleAsync(
        DeleteResourceCommand command,
        CancellationToken cancellationToken)
    {
        DeleteResourceValidator.Validate(command);
        var outcome = await repository.DeleteAsync(
            command,
            cancellationToken);

        switch (outcome)
        {
            case DeleteResourceOutcome.Deleted:
                return;
            case DeleteResourceOutcome.AlreadyDeleted:
                return; // Chỉ dùng nếu repeat-delete contract là 204.
            case DeleteResourceOutcome.NotFound:
                throw ResourceErrors.NotFound();
            case DeleteResourceOutcome.Conflict:
                throw ResourceErrors.InUse();
            case DeleteResourceOutcome.ConcurrencyConflict:
                throw ResourceErrors.ConcurrencyConflict();
            default:
                throw new InvalidOperationException("Unknown delete result.");
        }
    }
}
```

Nếu contract trả `404` cho already-deleted, map nhánh đó sang `NotFound`.
Authorization/business policy có thể cần load aggregate trước; giữ transaction và
race protection trong adapter.

### Soft-delete adapter shape

```csharp
// Pseudocode EF Core: thêm predicate current-state/version thực tế.
var affected = await db.Resources
    .Where(x => x.Id == command.ResourceId)
    .Where(x => x.DeletedAt == null)
    .Where(x => command.ExpectedVersion == null ||
                x.Version == command.ExpectedVersion)
    .ExecuteUpdateAsync(
        updates => updates
            .SetProperty(x => x.DeletedAt, clock.UtcNow)
            .SetProperty(x => x.DeletedBy, command.ActorId)
            .SetProperty(x => x.Version, x => x.Version + 1),
        cancellationToken);
```

Khi `affected == 0`, truy vấn phân loại not-found/already-deleted/concurrency.
Ghi outbox cùng transaction nếu delete phát event.

### Hard-delete adapter shape

```csharp
await using var transaction =
    await db.Database.BeginTransactionAsync(cancellationToken);

var resource = await db.Resources
    .SingleOrDefaultAsync(
        x => x.Id == command.ResourceId,
        cancellationToken);
if (resource is null)
{
    return DeleteResourceOutcome.NotFound;
}

if (await HasBlockingDependencyAsync(resource.Id, cancellationToken))
{
    return DeleteResourceOutcome.Conflict;
}

db.Resources.Remove(resource);
db.OutboxMessages.Add(ResourceDeletedMessage.From(resource));
await db.SaveChangesAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken);
return DeleteResourceOutcome.Deleted;
```

Database constraint vẫn phải chặn dependency race; map constraint violation sang
safe conflict, không trả tên constraint.

### Minimal API

```csharp
public static RouteHandlerBuilder MapDeleteResource(
    this IEndpointRouteBuilder endpoints) =>
    endpoints
        .MapDelete(
            ApiRoutes.Resources.ByIdTemplate,
            async (
                string resourceId,
                HttpContext context,
                DeleteResourceHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = DeleteResourceMapper.ToCommand(
                    resourceId,
                    context.User,
                    context.Request.Headers.IfMatch);
                await handler.HandleAsync(command, cancellationToken);
                return Results.NoContent();
            })
        .WithName("delete-resource")
        .WithTags(ServiceNames.Course)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);
```

Thay route/service/error metadata đúng contract, gồm `401/403/5xx` nếu thực tế.
Không khai báo success response type hay gửi envelope cho `204`.

## Ma trận test

- [ ] Unit: invalid ID/token, access denied và business state.
- [ ] Unit: first delete, not-found và already-deleted đúng status policy.
- [ ] Unit: dependency/in-use/state conflict và concurrency conflict.
- [ ] Unit: request lặp không nhân đôi audit/event/cleanup task.
- [ ] Component: route/header/auth và endpoint metadata.
- [ ] Component: `204` có body rỗng.
- [ ] Component: `404/409` chính xác, safe envelope có `traceId`.
- [ ] Integration soft: marker/audit/version và default query visibility.
- [ ] Integration hard: row/relations theo cascade/restrict contract.
- [ ] Integration: transaction rollback, dependency race, repeated/concurrent delete.
- [ ] Integration: outbox đúng một message và cleanup scheduling idempotent.

## Postman và quality gate

- [ ] DELETE request không có body, có route variable/auth/header cần thiết.
- [ ] Test first-delete khẳng định status `204` và response body rỗng.
- [ ] Test repeat-delete theo contract `204` hoặc `404`.
- [ ] Cases `404`, `409 dependency/state` và concurrency conflict.
- [ ] Nếu có GET kiểm chứng, không gộp endpoint mới; chỉ dùng request đã tồn tại
      làm bước xác minh.
- [ ] API doc và business-flow 1:1 đồng bộ.
- [ ] Feature/integration tests, `dotnet build`, `dotnet test` pass.
- [ ] `dotnet format --verify-no-changes` pass.
