// File: backend/Services/Media/MediaService.ComponentTests/Endpoints/Media/GetMediaLibraryEndpointComponentTests.cs
// Mục đích: Kiểm thử binding filter Media Library qua TestServer.

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Endpoints.Media;
using MediaService.Application;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.ComponentTests.Endpoints.Media;

public sealed class GetMediaLibraryEndpointComponentTests
{
    [Test]
    public async Task Get_StatusFilter_ReturnsFilteredEnvelope()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{ApiRoutes.BuildServicePath(ApiRoutes.Media.Library)}?mediaType=image&status=pending&pageSize=20");
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, "11111111-1111-1111-1111-111111111111");

        using var response = await fixture.Client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<MediaLibraryPageResponse>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            Assert.That(fixture.Repository.LastStatus, Is.EqualTo(MediaObjectStatuses.Pending));
            Assert.That(envelope?.Data.Items.Single().Status, Is.EqualTo(MediaObjectStatuses.Pending));
        });
    }

    [Test]
    public async Task GetReadyThumbnailReturnsNestedThumbnailMetadataAndContentUrl()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{ApiRoutes.BuildServicePath(ApiRoutes.Media.Library)}?mediaType=image&status=ready&pageSize=20");
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, "11111111-1111-1111-1111-111111111111");

        using var response = await fixture.Client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<MediaLibraryPageResponse>>();
        var thumbnail = envelope?.Data.Items.Single().Thumbnail;

        Assert.Multiple(() =>
        {
            Assert.That(thumbnail, Is.Not.Null);
            Assert.That(thumbnail!.Id, Is.EqualTo(Repository.ThumbnailMediaId));
            Assert.That(thumbnail.Status, Is.EqualTo(MediaDerivationStatuses.Ready));
            Assert.That(thumbnail.ContentType, Is.EqualTo("image/webp"));
            Assert.That(thumbnail.SizeBytes, Is.EqualTo(512));
            Assert.That(
                thumbnail.ContentUrl,
                Is.EqualTo(ApiRoutes.Media.ContentPublicPath(Repository.ThumbnailMediaId)));
        });
    }

    [Test]
    public async Task GetFailedThumbnailHidesThumbnailContentUrl()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{ApiRoutes.BuildServicePath(ApiRoutes.Media.Library)}?mediaType=image&status=failed&pageSize=20");
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, "11111111-1111-1111-1111-111111111111");

        using var response = await fixture.Client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<MediaLibraryPageResponse>>();
        var item = envelope?.Data.Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(item?.Thumbnail?.Status, Is.EqualTo(MediaDerivationStatuses.Failed));
            Assert.That(item?.Thumbnail?.ContentUrl, Is.Null);
            Assert.That(item?.ThumbnailUrl, Is.Null);
        });
    }

    private sealed class Repository : IMediaRepository
    {
        public static readonly Guid ThumbnailMediaId =
            Guid.Parse("33333333-3333-3333-3333-333333333333");

        public string? LastStatus { get; private set; }

        public Task<IReadOnlyList<MediaLibraryRecord>> ListByActorAsync(
            ActorReference actor,
            string? mediaType,
            string? status,
            (DateTime CreatedAtUtc, Guid Id)? cursor,
            int take,
            CancellationToken cancellationToken)
        {
            LastStatus = status;
            var ready = string.Equals(status, MediaObjectStatuses.Ready, StringComparison.Ordinal);
            var failed = string.Equals(status, MediaObjectStatuses.Failed, StringComparison.Ordinal);
            return Task.FromResult<IReadOnlyList<MediaLibraryRecord>>(
            [
                new MediaLibraryRecord(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    MediaTypes.Image,
                    "image/png",
                    ready ? "ready.png" : failed ? "failed.png" : "pending.png",
                    1,
                    ready
                        ? MediaObjectStatuses.Ready
                        : failed ? MediaObjectStatuses.Failed : MediaObjectStatuses.Pending,
                    true,
                    DateTime.UnixEpoch,
                    DateTime.UnixEpoch,
                    ready ? DateTime.UnixEpoch : null,
                    ready || failed
                        ? new MediaLibraryThumbnailRecord(
                            ThumbnailMediaId,
                            ready ? MediaObjectStatuses.Ready : MediaObjectStatuses.Failed,
                            ready ? MediaBackgroundJobStatuses.Completed : MediaBackgroundJobStatuses.Failed,
                            "image/webp",
                            512,
                            DateTime.UnixEpoch,
                            DateTime.UnixEpoch)
                        : null),
            ]);
        }

        public Task AddPendingAsync(PendingMediaRecord media, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaRecord?> GetByIdAsync(Guid mediaId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkFailedAsync(
            Guid mediaId,
            string failureReason,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task MarkReadyAsync(
            Guid mediaId,
            string checksumSha256,
            DateTime completedAtUtc,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class AcceptAdmin : IActorValidationService
    {
        public Task<ActorReference> ValidateAsync(
            ActorReference actor,
            CancellationToken cancellationToken) => Task.FromResult(actor.Normalize());
    }

    private sealed class Fixture(WebApplication app, HttpClient client, Repository repository) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public Repository Repository { get; } = repository;

        public static async Task<Fixture> CreateAsync()
        {
            var builder = WebApplication.CreateBuilder();
            var repository = new Repository();
            builder.WebHost.UseTestServer();
            builder.Services.AddMediaApplication(builder.Configuration);
            builder.Services.AddSingleton<IActorValidationService, AcceptAdmin>();
            builder.Services.AddSingleton<IMediaRepository>(repository);
            var app = builder.Build();
            app.UseSharedApiMiddleware();
            app.MapGetMediaLibrary();
            await app.StartAsync();
            return new Fixture(app, app.GetTestClient(), repository);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}
