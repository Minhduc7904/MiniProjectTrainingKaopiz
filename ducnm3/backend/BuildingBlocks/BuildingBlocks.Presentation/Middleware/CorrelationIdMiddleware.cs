using BuildingBlocks.Contracts.Api;

namespace BuildingBlocks.Presentation.Middleware;

/// <summary>
/// Đảm bảo mỗi request có correlation ID:
/// dùng header client gửi nếu có, nếu không tạo GUID mới.
/// Correlation ID được lưu vào HttpContext.TraceIdentifier
/// và echo lại trong response.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers[ApiHeaderNames.CorrelationId]
                .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.TraceIdentifier = correlationId;

        context.Request.Headers[
            ApiHeaderNames.CorrelationId] = correlationId;

        context.Response.Headers[
            ApiHeaderNames.CorrelationId] = correlationId;

        await next(context);
    }
}