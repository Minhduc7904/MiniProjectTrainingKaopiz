using BuildingBlocks.Contracts.Api;

namespace BuildingBlocks.Presentation.Middleware;

/// <summary>
/// Đảm bảo mỗi request có correlation ID: dùng header client gửi nếu có, nếu không tạo GUID mới.
/// Giá trị được lưu vào <see cref="HttpContext.TraceIdentifier"/> và echo trong response để truy vết end-to-end.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>Chuẩn hóa correlation ID trước khi gọi middleware kế tiếp; không trả giá trị riêng.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Ghi cả request lẫn response header để typed HTTP client và người gọi đều tiếp tục được cùng trace.
        var traceId = context.Request.Headers[ApiHeaderNames.CorrelationId].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(traceId))
        {
            traceId = Guid.NewGuid().ToString("N");
        }

        context.TraceIdentifier = traceId;
        context.Request.Headers[ApiHeaderNames.CorrelationId] = traceId;
        context.Response.Headers[ApiHeaderNames.CorrelationId] = traceId;

        await next(context);
    }
}
