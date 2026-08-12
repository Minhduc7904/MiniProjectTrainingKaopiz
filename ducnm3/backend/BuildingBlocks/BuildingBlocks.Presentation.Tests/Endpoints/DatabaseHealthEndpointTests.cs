using System.Net;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Presentation.Tests.Endpoints;

public class DatabaseHealthEndpointTests
{
    [TestCase(true, HttpStatusCode.OK, HealthStatusValues.Healthy)]
    [TestCase(false, HttpStatusCode.ServiceUnavailable, null)]
    public async Task HealthEndpointReturnsDatabaseStatusFromProbe(
        bool databaseHealthy,
        HttpStatusCode expectedStatusCode,
        string? expectedDatabaseStatus)
    {
        await using var app = await CreateApplicationAsync(databaseHealthy);
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

    private static async Task<WebApplication> CreateApplicationAsync(bool databaseHealthy)
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
}
