// File: backend/Services/Media/MediaService.ComponentTests/Endpoints/MediaUsageJobs/GetMediaBackgroundJobsEndpointComponentTests.cs
// Mục đích: Kiểm thử route, actor ADMIN, envelope và no-store của GET Media background jobs bằng TestServer.

#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.MediaUsageJobs;
using MediaService.Api.Endpoints.MediaUsageJobs.GetList;
using MediaService.Application;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Application.UseCases.MediaUsageJobs.GetList;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.ComponentTests.Endpoints.MediaUsageJobs;

public sealed class GetMediaBackgroundJobsEndpointComponentTests
{
    [Test]
    public async Task Get_AdminActor_ReturnsOffsetEnvelopeAndNoStore()
    {
        await using var fixture = await CreateAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.Media.JobsServicePath());
        request.Headers.Add(ApiHeaderNames.ActorType, ActorTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, "11111111-1111-1111-1111-111111111111");
        using var response = await fixture.Client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<MediaBackgroundJobListResponse>>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(envelope?.Data, Has.Count.EqualTo(1));
            Assert.That(envelope?.Meta.Pagination, Is.TypeOf<OffsetPaginationMeta>());
        });
    }

    private static async Task<Fixture> CreateAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddMediaApplication(builder.Configuration);
        builder.Services.AddSingleton<IMediaBackgroundJobListRepository>(new Repository());
        builder.Services.AddSingleton<IActorValidationService, AcceptAdmin>();
        var app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetMediaBackgroundJobs();
        await app.StartAsync();
        return new Fixture(app, app.GetTestClient());
    }

    private sealed class AcceptAdmin : IActorValidationService
    {
        public Task<ActorReference> ValidateAsync(ActorReference actor, CancellationToken cancellationToken) =>
            Task.FromResult(actor.Normalize());
    }

    private sealed class Repository : IMediaBackgroundJobListRepository
    {
        public Task<MediaBackgroundJobListPage> ListAsync(MediaBackgroundJobListRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new MediaBackgroundJobListPage([
                new MediaBackgroundJobListRecord(Guid.Parse("33333333-3333-3333-3333-333333333333"), MediaBackgroundJobTypes.NotificationUsage,
                    "NOTIFICATION_BATCH", Guid.Parse("44444444-4444-4444-4444-444444444444"), null, MediaBackgroundJobStatuses.Processing,
                    10, 3, 0, 1, null, DateTime.UnixEpoch, DateTime.UnixEpoch, null, DateTime.UnixEpoch),
            ], 1));
    }

    private sealed class Fixture(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;
        public async ValueTask DisposeAsync() { Client.Dispose(); await app.DisposeAsync(); }
    }
}
