using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Endpoints.Media;
using MediaService.Application;
using MediaService.Application.Abstractions.Clients;
using MediaService.Application.Abstractions.Derivation;
using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Abstractions.Storage;
using MediaService.Domain.Actors;
using MediaService.Domain.Media;
using MediaService.Domain.Usages;
using MediaService.UnitTests.TestDoubles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.UnitTests.Endpoints;

public sealed class MediaCommandEndpointTests
{
    private static readonly byte[] ContentBytes = [1, 2, 3];
    private readonly StubMediaRepository repository = new();
    private readonly StubStorage storage = new();
    private WebApplication app = null!;
    private HttpClient client = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IMediaRepository>(repository);
        builder.Services.AddSingleton<IMediaUploadFinalizer>(
            new ComponentUploadFinalizer(repository));
        builder.Services.AddSingleton<IMediaDerivationRepository>(
            new ComponentDerivationRepository());
        builder.Services.AddSingleton<IThumbnailGenerator, UnusedThumbnailGenerator>();
        builder.Services.AddSingleton<ITemporaryMediaFileFactory, UnusedTemporaryFileFactory>();
        builder.Services.AddSingleton<IStorage>(storage);
        builder.Services.AddSingleton<IStorageLocationAllocator, ComponentAllocator>();
        builder.Services.AddSingleton<IStudentLookup, StubStudentLookup>();
        builder.Services.AddMediaApplication(builder.Configuration);

        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapUploadMedia();
        app.MapCreateMediaUsage();
        app.MapGetMediaContent();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task MultipartUploadAndUsageReturnStandardCreatedResponses()
    {
        var actorId = Guid.NewGuid();
        using var multipart = CreateMultipart(actorId, ActorTypes.Student);

        using var uploadResponse = await client.PostAsync(
            ApiRoutes.Media.Upload,
            multipart);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(uploadResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(uploadBody, Does.Contain("\"status\":\"READY\""));
            Assert.That(
                uploadBody,
                Does.Contain("\"contentUrl\":\"/media/api/media/"));
            Assert.That(uploadBody, Does.Not.Contain("objectKey"));
            Assert.That(uploadBody, Does.Not.Contain("\"bucket\""));
        });

        var mediaId = repository.Pending!.Id;
        var ownerId = Guid.NewGuid();
        using var usageResponse = await client.PostAsJsonAsync(
            ApiRoutes.Media.Usages,
            new CreateMediaUsageRequest(
                mediaId.ToString(),
                MediaOwnerServices.Student,
                MediaOwnerTypes.StudentAvatar,
                ownerId.ToString(),
                MediaUsageTypes.Avatar,
                0,
                ActorTypes.Student,
                actorId.ToString()));
        var usageBody = await usageResponse.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(usageResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(usageBody, Does.Contain("\"ownerType\":\"STUDENT_AVATAR\""));
            Assert.That(repository.CreatedUsage!.OwnerId, Is.EqualTo(ownerId));
            Assert.That(repository.CreatedUsage.CreatedBy.Id, Is.EqualTo(actorId));
        });
    }

    [Test]
    public async Task UnknownActorTypeReturnsSafeValidationError()
    {
        using var multipart = CreateMultipart(Guid.NewGuid(), "ADMIN");

        using var response = await client.PostAsync(
            ApiRoutes.Media.Upload,
            multipart);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body, Does.Contain(MediaErrorCodes.InvalidActorType));
            Assert.That(body, Does.Not.Contain("stack"));
        });
    }

    [Test]
    public async Task UploadedMediaContentStreamsWithSafeHeaders()
    {
        var actorId = Guid.NewGuid();
        using var multipart = CreateMultipart(actorId, ActorTypes.Student);
        using var uploadResponse = await client.PostAsync(
            ApiRoutes.Media.Upload,
            multipart);
        Assert.That(uploadResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        using var contentResponse = await client.GetAsync(
            ApiRoutes.Media.ContentServicePath(repository.Pending!.Id));
        var bytes = await contentResponse.Content.ReadAsByteArrayAsync();

        Assert.Multiple(() =>
        {
            Assert.That(contentResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(contentResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/png"));
            Assert.That(contentResponse.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(bytes, Is.EqualTo(ContentBytes));
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private static MultipartFormDataContent CreateMultipart(
        Guid actorId,
        string actorType)
    {
        var multipart = new MultipartFormDataContent();
        var file = new ByteArrayContent(ContentBytes);
        file.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        multipart.Add(file, "file", "avatar.png");
        multipart.Add(new StringContent(MediaTypes.Image), "mediaType");
        multipart.Add(new StringContent(actorType), "uploadedByType");
        multipart.Add(new StringContent(actorId.ToString()), "uploadedBy");
        return multipart;
    }

    private sealed class ComponentAllocator : IStorageLocationAllocator
    {
        public StorageObjectLocation Allocate(
            StorageMediaCategory category,
            string extension) =>
            new("images", "2026/08/13/component.png");
    }

    private sealed class ComponentUploadFinalizer(StubMediaRepository repository)
        : IMediaUploadFinalizer
    {
        public async Task<MediaUploadFinalizationResult> FinalizeAsync(
            MediaUploadFinalizationRequest request,
            CancellationToken cancellationToken)
        {
            await repository.MarkReadyAsync(
                request.SourceMediaId,
                request.SourceChecksumSha256,
                request.CompletedAtUtc,
                cancellationToken);
            return new MediaUploadFinalizationResult(
                request.Thumbnail is null ? "NOT_REQUIRED" : "QUEUED",
                request.Thumbnail?.MediaId,
                request.Thumbnail?.JobId);
        }
    }

    private sealed class ComponentDerivationRepository
        : IMediaDerivationRepository
    {
        public Task<ThumbnailDerivationWork?> BeginAsync(
            Guid jobId,
            Guid sourceMediaId,
            Guid derivativeMediaId,
            CancellationToken cancellationToken) =>
            Task.FromResult<ThumbnailDerivationWork?>(null);

        public Task CompleteAsync(
            Guid jobId,
            Guid derivativeMediaId,
            string checksumSha256,
            long sizeBytes,
            DateTime completedAtUtc,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task MarkFailedAsync(
            Guid jobId,
            Guid derivativeMediaId,
            string safeError,
            DateTime failedAtUtc,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<MediaThumbnailStatusRecord?> GetThumbnailStatusAsync(
            Guid sourceMediaId,
            CancellationToken cancellationToken) =>
            Task.FromResult<MediaThumbnailStatusRecord?>(null);

        public Task<MediaThumbnailStatusRecord> RetryAsync(
            Guid sourceMediaId,
            ActorReference requestedBy,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedThumbnailGenerator : IThumbnailGenerator
    {
        public Task<GeneratedThumbnail> GenerateAsync(
            ThumbnailGenerationRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedTemporaryFileFactory : ITemporaryMediaFileFactory
    {
        public Task<ITemporaryMediaFile> CreateAsync(
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
