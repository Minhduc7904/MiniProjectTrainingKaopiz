using BuildingBlocks.Contracts.Api;

namespace BuildingBlocks.Presentation.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
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
