using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Presentation.Extensions;
using BuildingBlocks.Presentation.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Presentation.Tests;

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

    [TestCase(true, HttpStatusCode.OK, HealthStatusValues.Healthy)]
    [TestCase(false, HttpStatusCode.ServiceUnavailable, null)]
    public async Task HealthEndpointReturnsDatabaseStatusFromProbe(
        bool databaseHealthy,
        HttpStatusCode expectedStatusCode,
        string? expectedDatabaseStatus)
    {
        await using var app = await CreateHealthApplicationAsync(databaseHealthy);
        using var client = app.GetTestClient();

        var response = await client.GetAsync(ApiPaths.Health);

        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());

        if (databaseHealthy)
        {
            Assert.That(
                document.RootElement.GetProperty("data").GetProperty("database").GetProperty("status").GetString(),
                Is.EqualTo(expectedDatabaseStatus));
        }
        else
        {
            Assert.That(
                document.RootElement.GetProperty("error").GetProperty("code").GetString(),
                Is.EqualTo(ApiErrorCodes.DatabaseUnavailable));
        }
    }

    [Test]
    public async Task GatewaySwaggerDocumentReturnsServiceUnavailableWhenDownstreamRequestFails()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddHttpClient("course-service", client => client.BaseAddress = new Uri("http://course-service/"))
            .ConfigurePrimaryHttpMessageHandler<ThrowingHttpMessageHandler>();
        builder.Services.AddTransient<ThrowingHttpMessageHandler>();

        await using var app = builder.Build();
        app.MapGatewaySwaggerDocument("/course", "course-service");
        await app.StartAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/course/swagger/v1/swagger.json");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        Assert.That(
            document.RootElement.GetProperty("error").GetProperty("code").GetString(),
            Is.EqualTo(ApiErrorCodes.ServiceUnavailable));
    }

    private static async Task<WebApplication> CreateHealthApplicationAsync(bool databaseHealthy)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IDatabaseHealthProbe>(new StubDatabaseHealthProbe(databaseHealthy));

        var app = builder.Build();
        app.MapDatabaseHealthEndpoint("test-service");
        await app.StartAsync();
        return app;
    }

    private sealed class StubDatabaseHealthProbe(bool isHealthy) : IDatabaseHealthProbe
    {
        public Task<DatabaseHealthProbeResult> CheckAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new DatabaseHealthProbeResult(isHealthy));
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("Downstream service is unavailable.");
    }
}
