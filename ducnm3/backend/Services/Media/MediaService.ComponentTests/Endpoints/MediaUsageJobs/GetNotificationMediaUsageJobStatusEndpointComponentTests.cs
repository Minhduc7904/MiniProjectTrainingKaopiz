// File: backend/Services/Media/MediaService.ComponentTests/Endpoints/MediaUsageJobs/GetNotificationMediaUsageJobStatusEndpointComponentTests.cs
// Mục đích: Kiểm thử route, no-store envelope và 404 của GET Media Usage job status bằng TestServer.

#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.MediaUsageJobs;
using MediaService.Api.Endpoints.MediaUsageJobs.GetStatus;
using MediaService.Application;
using MediaService.Application.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.ComponentTests.Endpoints.MediaUsageJobs;

public sealed class GetNotificationMediaUsageJobStatusEndpointComponentTests
{
    [Test]
    public async Task Get_ExistingJob_ReturnsProgressEnvelopeAndNoStore()
    {
        var jobId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var repository = new JobRepository
        {
            Record = new NotificationMediaUsageJobRecord(
                jobId, "PROCESSING", 100, 40, 0,
                DateTime.UnixEpoch, DateTime.UnixEpoch, null, null),
        };
        await using var fixture = await CreateAsync(repository);

        using var response = await fixture.Client.GetAsync(
            ApiRoutes.Media.NotificationMediaUsageJobStatusServicePath(jobId));
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<NotificationMediaUsageJobStatusResponse>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(envelope?.Data.ProgressPercent, Is.EqualTo(40m));
            Assert.That(envelope?.Data.RemainingUsageCount, Is.EqualTo(60));
        });
    }

    private static async Task<JobApiFixture> CreateAsync(JobRepository repository)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddMediaApplication(builder.Configuration);
        builder.Services.AddSingleton<INotificationMediaUsageJobRepository>(repository);
        var app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetNotificationMediaUsageJobStatus();
        await app.StartAsync();
        return new JobApiFixture(app, app.GetTestClient());
    }

    private sealed class JobRepository : INotificationMediaUsageJobRepository
    {
        public NotificationMediaUsageJobRecord? Record { get; init; }
        public Task<NotificationMediaUsageJobRecord?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken) =>
            Task.FromResult(Record?.Id == jobId ? Record : null);
        public Task StartAsync(Guid jobId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task RecordSuccessAsync(Guid jobId, uint usageCount, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task RecordFailureAsync(Guid jobId, uint usageCount, string safeError, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task CompleteSourceAsync(Guid jobId, uint expectedUsageCount, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class JobApiFixture(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;
        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}
