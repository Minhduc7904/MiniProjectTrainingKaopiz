# Template GET list endpoint

Thay placeholder `<...>`. Chọn **một** nhánh cursor hoặc offset và xóa nhánh
không dùng khỏi implementation/doc của endpoint.

## Phiếu chốt contract

```text
Owner service:
Direct/public path:
Authentication/visibility:
Item fields:
Filters + normalization:
Allowed sort keys/directions:
Default sort:
Unique tie-breaker/null order:
Pagination: cursor | offset
Limit/pageSize default + max:
Invalid query error codes:
Cache policy:
Consistency khi dữ liệu thay đổi:
```

## API doc

Tạo đúng một file `docs/api/<owning-service>/endpoints/get-<resources>.md`:

````markdown
# `GET /<gateway-prefix>/api/<resources>`

## Mục đích
<Mục tiêu list/search và owner>. Direct path là `GET /api/<resources>`.

## Xác thực và phân quyền
- Xác thực: <...>.
- Visibility/ownership: <...>.

## Yêu cầu

| Query | Kiểu | Mặc định | Quy tắc |
| --- | --- | --- | --- |
| `<filter>` | string | Không có | <allowlist/normalization>. |
| `sort` | string | `<default>` | <allowed keys>. |
| `direction` | string | `asc` | `asc` hoặc `desc`. |
| `<cursor hoặc page>` | ... | ... | ... |

Không có request body. Filter kết hợp theo <AND/OR>.

## Phản hồi thành công

`200 OK`

```json
{
  "data": [],
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "<cursor|offset>"
    }
  }
}
```

<Điền đầy đủ fields cursor hoặc offset theo response-format.md>.

## Sắp xếp và phân trang
- Total order: `<sort-key> <direction>, id <direction>`.
- Null/collation: <...>.
- Cursor semantics hoặc offset consistency: <...>.

## Mã trạng thái HTTP
- `200`: kể cả list rỗng.
- `400 <INVALID_...>`: <filter/sort/page/cursor>.
- <Auth/dependency/5xx thực tế>.

## Điều kiện nghiệp vụ và tác động phụ
- GET safe, idempotent, chỉ đọc.
- Cache: <...>.
- Đọc <bảng/dependency>; không thay đổi dữ liệu.
````

## Business flow 1:1

Tạo đúng một file `docs/business-flows/<domain>/get-<resources>.md`:

```markdown
# Liệt kê <resources>

## Mục đích
<...>

## Tác nhân và điều kiện đầu vào
- <Actor/visibility>.
- <Filter/sort/page hợp lệ>.

## Luồng chính
1. Client gửi GET không body với filter/sort/page.
2. API bind và Application validate/normalize query.
3. Infrastructure áp dụng visibility, filter, total order và pagination.
4. API trả `200` envelope với metadata.
5. <Cursor: client dùng nextCursor | Offset: client chọn page>.

## Trường hợp rỗng và lỗi
- Không có item: `200`, `data: []`.
- Query không hợp lệ: <...>.
- <...>.

## Tính nhất quán
<Semantics khi insert/delete/update giữa các page>.

## Dữ liệu thay đổi
Không có; endpoint chỉ đọc <...>.
```

## Shared pagination contract skeleton

Chỉ dùng nếu shared contracts chưa hỗ trợ `meta.pagination`:

```csharp
public sealed record ApiResponse<TData>(TData Data, ResponseMeta Meta);

public sealed record ResponseMeta(
    string TraceId,
    PaginationMeta? Pagination = null);

public abstract record PaginationMeta(string Type);

public sealed record CursorPaginationMeta(
    int Limit,
    string? NextCursor,
    bool HasNextPage) : PaginationMeta("cursor");

public sealed record OffsetPaginationMeta(
    int Page,
    int PageSize,
    long TotalItems,
    int TotalPages) : PaginationMeta("offset");
```

Điều chỉnh serialization theo convention và thêm shared compatibility tests.
Không copy các type này vào từng service.

## Application skeleton

```csharp
public enum <Resource>Sort
{
    CreatedAt,
    Name,
}

public sealed record Get<Resources>Query(
    string? Status,
    <Resource>Sort Sort,
    bool Descending,
    int Limit,
    string? Cursor);

public sealed record <Resource>ListItem(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc);

public sealed record Get<Resources>Result(
    IReadOnlyList<<Resource>ListItem> Items,
    string? NextCursor,
    bool HasNextPage);

public interface I<Resource>ListRepository
{
    Task<Get<Resources>Result> GetListAsync(
        Get<Resources>Query query,
        CancellationToken cancellationToken);
}
```

Validator phải enforce filter/sort allowlist, `limit` boundary và cursor
integrity trước khi repository chạy.

## Cursor adapter skeleton

```csharp
public async Task<Get<Resources>Result> GetListAsync(
    Get<Resources>Query query,
    CancellationToken cancellationToken)
{
    var position = cursorCodec.Decode(query.Cursor, query.Sort, query.Descending);
    var source = dbContext.<Resources>
        .AsNoTracking()
        .Where(item => item.DeletedAt == null);

    source = ApplyFilters(source, query);
    source = ApplyCursorPredicate(source, position, query);
    source = ApplyStableOrder(source, query); // sort key rồi Id

    var rows = await source
        .Take(query.Limit + 1)
        .Select(item => new <Resource>ListItem(
            item.Id,
            item.Name,
            item.CreatedAt))
        .ToListAsync(cancellationToken);

    var hasNextPage = rows.Count > query.Limit;
    var items = rows.Take(query.Limit).ToArray();
    var nextCursor = hasNextPage
        ? cursorCodec.Encode(items[^1], query.Sort, query.Descending)
        : null;
    return new(items, nextCursor, hasNextPage);
}
```

Cursor phải chứa đủ sort position + unique tie-breaker và được version/integrity
check. Predicate cho descending phải đảo đúng toán tử.

## Offset adapter skeleton

```csharp
public sealed record Get<Resources>OffsetQuery(
    string? Status,
    <Resource>Sort Sort,
    bool Descending,
    int Page,
    int PageSize);

public sealed record Get<Resources>OffsetResult(
    IReadOnlyList<<Resource>ListItem> Items,
    long TotalItems,
    int TotalPages);

public async Task<Get<Resources>OffsetResult> GetListAsync(
    Get<Resources>OffsetQuery query,
    CancellationToken cancellationToken)
{
    var source = ApplyFilters(
        dbContext.<Resources>.AsNoTracking()
            .Where(item => item.DeletedAt == null),
        query);
    var ordered = ApplyStableOrder(source, query);
    var totalItems = await source.LongCountAsync(cancellationToken);
    var skip = checked((query.Page - 1) * query.PageSize);
    var items = await ordered
        .Skip(skip)
        .Take(query.PageSize)
        .Select(item => new <Resource>ListItem(
            item.Id,
            item.Name,
            item.CreatedAt))
        .ToListAsync(cancellationToken);
    var totalPages = totalItems == 0
        ? 0
        : checked((int)Math.Ceiling(totalItems / (double)query.PageSize));
    return new(items, totalItems, totalPages);
}
```

## API endpoint skeleton

```csharp
public static RouteHandlerBuilder MapGet<Resources>(
    this IEndpointRouteBuilder endpoints) =>
    endpoints
        .MapGet(
            ApiRoutes.<Resources>.List,
            async (
                string? status,
                string? sort,
                string? direction,
                int? limit,
                string? cursor,
                HttpContext context,
                Get<Resources>Handler handler,
                CancellationToken cancellationToken) =>
            {
                var query = Get<Resources>QueryMapper.Map(
                    status,
                    sort,
                    direction,
                    limit,
                    cursor);
                var result = await handler.HandleAsync(
                    query,
                    cancellationToken);
                context.Response.Headers.CacheControl = "no-store";

                return Results.Json(
                    ApiResponseFactory.Success(
                        result.Items,
                        context.TraceIdentifier,
                        new CursorPaginationMeta(
                            query.Limit,
                            result.NextCursor,
                            result.HasNextPage)));
            })
        .WithName("get-<resources>")
        .WithTags(ServiceNames.<Service>)
        .Produces<ApiResponse<IReadOnlyList<<Resource>ListItem>>>(
            StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
```

Đổi parameters/metadata sang offset nếu đã chọn offset. Skeleton giả định đã
thêm overload factory phù hợp; điều chỉnh theo shared contract thực tế.

## Test skeleton

```csharp
[Test]
public async Task DuplicateSortValuesUseStableIdTieBreaker()
{
    var firstPage = await repository.GetListAsync(
        query with { Limit = 2 },
        CancellationToken.None);
    var secondPage = await repository.GetListAsync(
        query with { Cursor = firstPage.NextCursor },
        CancellationToken.None);

    Assert.Multiple(() =>
    {
        Assert.That(firstPage.Items, Has.Count.EqualTo(2));
        Assert.That(
            firstPage.Items.Select(item => item.Id)
                .Intersect(secondPage.Items.Select(item => item.Id)),
            Is.Empty);
    });
}

[Test]
public async Task EmptyListReturnsSuccessWithPaginationMetadata()
{
    using var response = await client.GetAsync(
        $"{ApiRoutes.<Resources>.List}?limit=20");
    var body = await response.Content.ReadAsStringAsync();

    Assert.Multiple(() =>
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(body, Does.Contain("\"data\":[]"));
        Assert.That(body, Does.Contain("\"pagination\""));
    });
}
```

Thêm tests invalid filter/sort/cursor/page, page boundaries và concurrent data
semantics bằng database container.

## Postman skeleton

```json
{
  "name": "List <resources>",
  "request": {
    "method": "GET",
    "header": [],
    "url": {
      "raw": "{{<service>BaseUrl}}/api/<resources>?status={{status}}&sort={{sort}}&limit={{limit}}",
      "query": []
    }
  }
}
```

Cursor test script lưu:

```javascript
const body = pm.response.json();
pm.collectionVariables.set("<resource>Cursor", body.meta.pagination.nextCursor);
```

Offset request thay `limit/cursor` bằng `page/pageSize`; kiểm tra
`totalItems/totalPages`.

## Checklist bàn giao

- [ ] Đã đọc reference/template và ba test skill.
- [ ] Chỉ GET list, không body, safe/idempotent.
- [ ] Filter/sort được allowlist và document.
- [ ] Có total order với unique tie-breaker.
- [ ] Chỉ dùng cursor hoặc offset, metadata đúng shared contract.
- [ ] Empty list trả `200`.
- [ ] Unit/component/integration coverage đủ boundary và concurrency semantics.
- [ ] Đúng một API doc và một business-flow file 1:1.
- [ ] Postman JSON hợp lệ, test được page tiếp theo.
- [ ] Build, full test và format verify đều đạt.
