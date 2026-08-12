using System.Net;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using MediaService.Api.Endpoints;
using MediaService.Application.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.UnitTests.Endpoints;

public class MediaHealthEndpointTests
{
    [TestCase(true, true, HttpStatusCode.OK, null)]
    [TestCase(false, true, HttpStatusCode.ServiceUnavailable, ApiErrorCodes.DatabaseUnavailable)]
    [TestCase(true, false, HttpStatusCode.ServiceUnavailable, ApiErrorCodes.StorageUnavailable)]
    [TestCase(false, false, HttpStatusCode.ServiceUnavailable, ApiErrorCodes.DependencyUnavailable)]
    public async Task HealthEndpointReportsEachDependencyCombination(
        bool databaseHealthy,
        bool storageHealthy,
        HttpStatusCode expectedStatusCode,
        string? expectedErrorCode)
    {
        await using var app = await CreateApplicationAsync(databaseHealthy, storageHealthy);
        using var client = app.GetTestClient();

        var response = await client.GetAsync(ApiPaths.Health);

        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());

        if (expectedErrorCode is not null)
        {
            Assert.That(
                document.RootElement.GetProperty("error").GetProperty("code").GetString(),
                Is.EqualTo(expectedErrorCode));
            return;
        }

        var data = document.RootElement.GetProperty("data");
        Assert.Multiple(() =>
        {
            Assert.That(data.GetProperty("status").GetString(), Is.EqualTo(HealthStatusValues.Healthy));
            Assert.That(
                data.GetProperty("database").GetProperty("status").GetString(),
                Is.EqualTo(HealthStatusValues.Healthy));
            Assert.That(
                data.GetProperty("storage").GetProperty("status").GetString(),
                Is.EqualTo(HealthStatusValues.Healthy));
        });
    }

    private static async Task<WebApplication> CreateApplicationAsync(
        bool databaseHealthy,
        bool storageHealthy)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IDatabaseHealthProbe>(
            new StubDatabaseHealthProbe(databaseHealthy));
        builder.Services.AddSingleton<IStorageHealthProbe>(
            new StubStorageHealthProbe(storageHealthy));

        var app = builder.Build();
        app.MapMediaHealthEndpoint();
        await app.StartAsync();
        return app;
    }

    private sealed class StubDatabaseHealthProbe(bool isHealthy) : IDatabaseHealthProbe
    {
        public Task<DatabaseHealthProbeResult> CheckAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new DatabaseHealthProbeResult(isHealthy));
    }

    private sealed class StubStorageHealthProbe(bool isHealthy) : IStorageHealthProbe
    {
        public Task<StorageHealthProbeResult> CheckAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new StorageHealthProbeResult(isHealthy));
    }
}
