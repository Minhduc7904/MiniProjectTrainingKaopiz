// File: backend/Services/Media/MediaService.ComponentTests/Endpoints/MediaUsages/CreateMediaUsageEndpointComponentTests.cs
// Mục đích: Kiểm thử endpoint tạo media usage từ chối thumbnail derivative qua HTTP.

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Endpoints.Media;
using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Application.Services.Storage;
using MediaService.Application.UseCases.MediaUsages.Create;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.ComponentTests.Endpoints;

public sealed class CreateMediaUsageEndpointComponentTests
{
    private static readonly Guid SourceMediaId = Guid.Parse("08f48484-d12e-420c-b075-2d83c4148f2f");
    private static readonly Guid ThumbnailMediaId = Guid.Parse("c3fe0c55-a935-4600-bcc2-9c860282f2a7");
    private static readonly Guid AdminId = Guid.Parse("8c9bdc15-66f2-4db2-aaf2-0e119bf6e8a8");
    private WebApplication app = null!;
    private HttpClient client = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        var repository = new DerivativeMediaRepository();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddScoped<CreateMediaUsageHandler>();
        builder.Services.AddSingleton<IActorValidationService, AcceptAdminActor>();
        builder.Services.AddSingleton<IMediaRepository>(repository);
        builder.Services.AddSingleton<IMediaUsageRepository>(repository);

        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapCreateMediaUsage();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Test]
    public async Task PostThumbnailDerivativeUsageReturnsInvalidMediaEnvelope()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Media.Usages)
        {
            Content = JsonContent.Create(new CreateMediaUsageRequest(
                ThumbnailMediaId.ToString("D"),
                MediaOwnerServices.Media,
                MediaOwnerTypes.MediaThumbnail,
                SourceMediaId.ToString("D"),
                MediaUsageTypes.Thumbnail,
                0)),
        };
        request.Headers.Add("X-Actor-Type", ActorTypes.Admin);
        request.Headers.Add("X-Actor-Id", AdminId.ToString("D"));

        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(MediaErrorCodes.InvalidMedia));
        });
    }

    private sealed class AcceptAdminActor : IActorValidationService
    {
        public Task<ActorReference> ValidateAsync(ActorReference actor, CancellationToken cancellationToken) =>
            Task.FromResult(actor.Normalize());
    }

    private sealed class DerivativeMediaRepository : IMediaRepository, IMediaUsageRepository
    {
        public Task AddPendingAsync(PendingMediaRecord media, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaRecord?> GetByIdAsync(Guid mediaId, CancellationToken cancellationToken) =>
            Task.FromResult<MediaRecord?>(mediaId == ThumbnailMediaId
                ? new MediaRecord(
                    ThumbnailMediaId,
                    new StorageObjectLocation("images", "thumbnail.webp"),
                    MediaTypes.Image,
                    "image/webp",
                    "thumbnail.webp",
                    3,
                    MediaObjectStatuses.Ready,
                    DateTime.UnixEpoch,
                    null,
                    SourceMediaId: SourceMediaId,
                    DerivationType: MediaDerivationTypes.Thumbnail)
                : null);

        public Task<IReadOnlyList<MediaLibraryRecord>> ListByActorAsync(
            ActorReference actor,
            string? mediaType,
            string? status,
            (DateTime CreatedAtUtc, Guid Id)? cursor,
            int take,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkFailedAsync(Guid mediaId, string failureReason, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkReadyAsync(Guid mediaId, string checksumSha256, DateTime completedAtUtc, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaService.Domain.Entities.MediaUsage> ReplaceStudentAvatarAsync(
            CreateMediaUsageRecord usage,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaService.Domain.Entities.MediaUsage> ReplaceCourseThumbnailAsync(
            CreateMediaUsageRecord usage,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaService.Domain.Entities.MediaUsage> AddCourseGalleryMediaAsync(
            CreateMediaUsageRecord usage,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task EnsureMediaUsagesAsync(
            IReadOnlyList<CreateMediaUsageRecord> usages,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaUsageUrlRecord?> GetActiveUsageUrlByIdAsync(Guid usageId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(
            MediaUsageOwnerQuery query,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Guid>> GetActiveUsageIdsByOwnersAsync(
            IReadOnlyList<MediaUsageOwnerScope> owners,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
