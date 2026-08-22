using System.Net;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Presentation.Tests.Gateway;

public class GatewayCorsTests
{
    private const string FrontendOrigin = "http://localhost:5173";
    private const string VercelOrigin = "https://mini-project-training-kaopiz.vercel.app";

    [Test]
    public async Task OptionsPreflightAllowedOriginReturnsNoContentWithCorsHeaders()
    {
        await using var app = await StartAsync();
        using var client = app.GetTestClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            ApiRoutes.Students.ListPublicPath());
        request.Headers.TryAddWithoutValidation("Origin", FrontendOrigin);
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "GET");
        request.Headers.TryAddWithoutValidation(
            "Access-Control-Request-Headers",
            ApiHeaderNames.CorrelationId);

        using var response = await client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(
            response.Headers.GetValues("Access-Control-Allow-Origin").Single(),
            Is.EqualTo(FrontendOrigin));
        Assert.That(
            string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")),
            Does.Contain(ApiHeaderNames.CorrelationId).IgnoreCase);
    }

    [Test]
    public async Task GetAllowedOriginIncludesAllowOriginHeader()
    {
        await using var app = await StartAsync();
        using var client = app.GetTestClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            ApiRoutes.Students.ListPublicPath());
        request.Headers.TryAddWithoutValidation("Origin", FrontendOrigin);

        using var response = await client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(
            response.Headers.GetValues("Access-Control-Allow-Origin").Single(),
            Is.EqualTo(FrontendOrigin));
    }

    [Test]
    public async Task OptionsPreflightVercelOriginReturnsNoContentWithCorsHeaders()
    {
        await using var app = await StartAsync();
        using var client = app.GetTestClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            ApiRoutes.Students.ListPublicPath());
        request.Headers.TryAddWithoutValidation("Origin", VercelOrigin);
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "GET");

        using var response = await client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(
            response.Headers.GetValues("Access-Control-Allow-Origin").Single(),
            Is.EqualTo(VercelOrigin));
    }

    [Test]
    public async Task GetUnknownOriginDoesNotEchoOrigin()
    {
        await using var app = await StartAsync();
        using var client = app.GetTestClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            ApiRoutes.Students.ListPublicPath());
        request.Headers.TryAddWithoutValidation("Origin", "http://evil.example");

        using var response = await client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
    }

    private static async Task<WebApplication> StartAsync()
    {
        var configurationPrefix = $"GatewayCorsTests_{Guid.NewGuid():N}_";
        Environment.SetEnvironmentVariable(
            $"{configurationPrefix}Cors__AllowedOrigins__0",
            FrontendOrigin);
        Environment.SetEnvironmentVariable(
            $"{configurationPrefix}Cors__AllowedOrigins__1",
            VercelOrigin);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddEnvironmentVariables(configurationPrefix);

        try
        {
            builder.Services.AddLmsCors(builder.Configuration);

            var app = builder.Build();
            app.UseLmsCors();
            app.MapGet(
                ApiRoutes.Students.ListPublicPath(),
                () => Results.Ok());
            await app.StartAsync();
            return app;
        }
        finally
        {
            Environment.SetEnvironmentVariable($"{configurationPrefix}Cors__AllowedOrigins__0", null);
            Environment.SetEnvironmentVariable($"{configurationPrefix}Cors__AllowedOrigins__1", null);
        }
    }
}
