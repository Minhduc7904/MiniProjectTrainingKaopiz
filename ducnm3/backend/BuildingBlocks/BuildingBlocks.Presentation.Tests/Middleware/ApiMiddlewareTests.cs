using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Presentation.Tests.Middleware;

public class ApiMiddlewareTests
{
    [Test]
    public async Task CorrelationIdMiddlewareGeneratesAndPropagatesTraceIdentifier()
    {
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.TraceIdentifier, Is.Not.Empty);
            Assert.That(
                context.Request.Headers[ApiHeaderNames.CorrelationId].ToString(),
                Is.EqualTo(context.TraceIdentifier));
            Assert.That(
                context.Response.Headers[ApiHeaderNames.CorrelationId].ToString(),
                Is.EqualTo(context.TraceIdentifier));
        });
    }

    [Test]
    public async Task ExceptionMiddlewareReturnsStandardServiceUnavailableEnvelope()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-id"
        };
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var middleware = new ApiExceptionHandlingMiddleware(
            _ => throw new ServiceUnavailableException(
                ApiErrorCodes.ServiceUnavailable,
                ApiErrorMessages.ServiceUnavailable),
            NullLogger<ApiExceptionHandlingMiddleware>.Instance,
            Options.Create(new JsonOptions()));

        await middleware.InvokeAsync(context);

        responseBody.Position = 0;
        using var document = await JsonDocument.ParseAsync(responseBody);

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
            Assert.That(
                document.RootElement.GetProperty("error").GetProperty("code").GetString(),
                Is.EqualTo(ApiErrorCodes.ServiceUnavailable));
            Assert.That(
                document.RootElement.GetProperty("meta").GetProperty("traceId").GetString(),
                Is.EqualTo("trace-id"));
        });
    }
}
