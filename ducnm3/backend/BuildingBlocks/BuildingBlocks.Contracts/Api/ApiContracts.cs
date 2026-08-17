using System.Text.Json.Serialization;

namespace BuildingBlocks.Contracts.Api;

/// <summary>Response envelope thành công; <paramref name="TData"/> là dữ liệu nghiệp vụ, còn <c>Meta</c> chứa trace và phân trang.</summary>
public sealed record ApiResponse<TData>(TData Data, ResponseMeta Meta);

/// <summary>Response envelope lỗi an toàn, được middleware dùng thay cho việc lộ exception nội bộ.</summary>
public sealed record ApiErrorResponse(ApiError Error, ResponseMeta Meta);

/// <summary>Metadata dùng chung của response. Pagination chỉ được serialize khi endpoint trả danh sách.</summary>
public sealed record ResponseMeta(
    string TraceId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    PaginationMeta? Pagination = null);

[JsonDerivedType(typeof(CursorPaginationMeta), "cursor")]
[JsonDerivedType(typeof(OffsetPaginationMeta), "offset")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
/// <summary>Base type phân biệt hai chiến lược phân trang trong JSON bằng discriminator <c>type</c>.</summary>
public abstract record PaginationMeta(
    [property: JsonIgnore] string Type);

/// <summary>Metadata cho cursor pagination; client dùng <c>NextCursor</c> để lấy trang kế tiếp khi <c>HasNextPage</c> là true.</summary>
public sealed record CursorPaginationMeta(
    int Limit,
    string? NextCursor,
    bool HasNextPage) : PaginationMeta("cursor");

/// <summary>Metadata cho offset pagination, bao gồm tổng số bản ghi và tổng số trang.</summary>
public sealed record OffsetPaginationMeta(
    int Page,
    int PageSize,
    long TotalItems,
    int TotalPages) : PaginationMeta("offset");

/// <summary>Thông tin lỗi chuẩn gồm code ổn định, thông điệp an toàn và chi tiết validation theo field.</summary>
public sealed record ApiError(
    string Code,
    string Message,
    IReadOnlyList<ApiErrorDetail> Details);

/// <summary>Một lỗi gắn với field cụ thể, chủ yếu dùng cho validation response.</summary>
public sealed record ApiErrorDetail(string Field, string Message);

/// <summary>Health response cho service có database và messaging dependency.</summary>
public sealed record ServiceHealthResponse(
    string Service,
    string Status,
    DatabaseHealthResponse Database,
    MessagingHealthResponse? Messaging = null);

/// <summary>Trạng thái database dependency.</summary>
public sealed record DatabaseHealthResponse(string Status);

/// <summary>Trạng thái object storage dependency.</summary>
public sealed record StorageHealthResponse(string Status);

/// <summary>Trạng thái RabbitMQ/MassTransit dependency.</summary>
public sealed record MessagingHealthResponse(string Status);

/// <summary>Health response riêng cho Media Service vì service này kiểm tra thêm storage.</summary>
public sealed record MediaServiceHealthResponse(
    string Service,
    string Status,
    DatabaseHealthResponse Database,
    StorageHealthResponse Storage,
    MessagingHealthResponse? Messaging = null);

/// <summary>Thông tin tối thiểu được trả về tại root endpoint của một service.</summary>
public sealed record ServiceInfoResponse(string Service, string Status);
