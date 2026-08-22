// File: backend/Services/Media/MediaService.ComponentTests/Endpoints/Media/GetMediaSummaryEndpointComponentTests.cs
// Mục đích: Kiểm thử route, actor ADMIN, envelope và no-store của GET Media summary.

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Endpoints.Media.GetSummary;
using MediaService.Application;
using MediaService.Application.Repositories;
using MediaService.Application.UseCases.Media.GetSummary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.ComponentTests.Endpoints.Media;

public sealed class GetMediaSummaryEndpointComponentTests
{
    [Test]
    public async Task Get_AdminActor_ReturnsSummaryEnvelopeAndNoStore()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.Media.SummaryServicePath());
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, "11111111-1111-1111-1111-111111111111");

        using var response = await fixture.Client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<MediaSummaryResult>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(envelope?.Data.TotalMedia, Is.EqualTo(9));
        });
    }

    private sealed class Repository : IMediaSummaryRepository
    {
        public Task<long> CountAsync(CancellationToken cancellationToken) => Task.FromResult(9L);
    }

    private sealed class Fixture(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public static async Task<Fixture> CreateAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddMediaApplication(builder.Configuration);
            builder.Services.AddSingleton<IMediaSummaryRepository, Repository>();
            var app = builder.Build();
            app.UseSharedApiMiddleware();
            app.MapGetMediaSummary();
            await app.StartAsync();
            return new Fixture(app, app.GetTestClient());
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}
