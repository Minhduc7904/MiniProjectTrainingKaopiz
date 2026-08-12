using System.Net;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Presentation.Tests.Gateway;

public class GatewaySwaggerDocumentTests
{
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

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("Downstream service is unavailable.");
    }
}
