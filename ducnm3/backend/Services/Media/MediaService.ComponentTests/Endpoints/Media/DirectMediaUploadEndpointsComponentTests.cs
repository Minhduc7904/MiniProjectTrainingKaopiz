// File: backend/Services/Media/MediaService.ComponentTests/Endpoints/Media/DirectMediaUploadEndpointsComponentTests.cs
// Mục đích: Kiểm thử endpoint upload trực tiếp qua TestServer: intent, complete, route, envelope và các response lỗi.

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Endpoints.Media;
using MediaService.Application;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.Services.Actors;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Domain.ValueObjects;
using MediaService.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using MediaService.Application.Common.Errors;

namespace MediaService.ComponentTests.Endpoints;

public sealed class DirectMediaUploadEndpointsComponentTests
{
    private static readonly Guid ActorId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
    private WebApplication app = null!;
    private HttpClient client = null!;
    private DirectRepository repository = null!;
    private FixedStorage storage = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        var builder = WebApplication.CreateBuilder();
        repository = new DirectRepository();
        storage = new FixedStorage();
        builder.WebHost.UseTestServer();
        builder.Services.AddMediaApplication(builder.Configuration);
        builder.Services.AddSingleton<IActorValidationService, AcceptActor>();
        builder.Services.AddSingleton<IStorageLocationAllocator, FixedAllocator>();
        builder.Services.AddSingleton<IStorageUploadPolicyProvider, FixedPolicy>();
        builder.Services.AddSingleton<IStorage>(storage);
        builder.Services.AddSingleton<IMediaRepository>(repository);
        builder.Services.AddSingleton<IMediaUploadFinalizer>(new FixedFinalizer(repository));
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapCreateUploadIntent();
        app.MapCompleteDirectUpload();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task PostIntentValidRequestReturnsCreatedEnvelopeAndCanonicalLocation()
    {
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.BuildServicePath(ApiRoutes.Media.UploadIntents),
            new CreateUploadIntentRequest(
                "photo.png", "IMAGE", "image/png", 3, new string('a', 64),
                ActorId.ToString("D"), "STUDENT"),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<CreateUploadIntentResponse>>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Headers.Location?.OriginalString,
                Is.EqualTo(ApiRoutes.Media.ResourcePublicPath(envelope!.Data.MediaId)));
            Assert.That(envelope.Data.Status, Is.EqualTo(MediaObjectStatuses.Pending));
            Assert.That(envelope.Data.IsDraft, Is.True);
        });
    }

    [Test]
    public async Task PostIntentInvalidChecksumReturnsSafeValidationEnvelope()
    {
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.BuildServicePath(ApiRoutes.Media.UploadIntents),
            new CreateUploadIntentRequest(
                "photo.png", "IMAGE", "image/png", 3, "BAD",
                ActorId.ToString("D"), "STUDENT"),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(MediaErrorCodes.InvalidMedia));
        });
    }

    [Test]
    public async Task PostCompleteValidThenRetryReturnsReadyEnvelopeWithoutStorageRouting()
    {
        var mediaId = await CreateIntentAsync();
        var request = new CompleteDirectUploadRequest(
            ActorId.ToString("D"), "STUDENT");

        using var first = await client.PostAsJsonAsync(
            ApiRoutes.Media.UploadCompleteServicePath(mediaId), request,
            TestContext.CurrentContext.CancellationToken);
        using var second = await client.PostAsJsonAsync(
            ApiRoutes.Media.UploadCompleteServicePath(mediaId), request,
            TestContext.CurrentContext.CancellationToken);
        var envelope = await second.Content.ReadFromJsonAsync<
            ApiResponse<UploadMediaResponse>>(
            TestContext.CurrentContext.CancellationToken);
        var body = await second.Content.ReadAsStringAsync(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(envelope?.Data.Status, Is.EqualTo(MediaObjectStatuses.Ready));
            Assert.That(envelope?.Data.IsDraft, Is.True);
            Assert.That(repository.Media!.Location.ObjectKey,
                Is.EqualTo("internal/object2.png"));
            Assert.That(body, Does.Not.Contain("internal/object"));
            Assert.That(body, Does.Not.Contain("\"bucket\""));
            Assert.That(body, Does.Not.Contain("\"objectKey\""));
        });
    }

    [Test]
    public async Task PostCompleteDifferentOwnerReturnsNotFoundEnvelope()
    {
        var mediaId = await CreateIntentAsync();
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.Media.UploadCompleteServicePath(mediaId),
            new CompleteDirectUploadRequest(Guid.NewGuid().ToString("D"), "STUDENT"),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(envelope?.Error.Code, Is.EqualTo(MediaErrorCodes.MediaNotFound));
        });
    }

    [Test]
    public async Task PostCompleteMetadataMismatchReturnsSafeConflictEnvelope()
    {
        var mediaId = await CreateIntentAsync();
        storage.Size = 4;
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.Media.UploadCompleteServicePath(mediaId),
            new CompleteDirectUploadRequest(ActorId.ToString("D"), "STUDENT"),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
            TestContext.CurrentContext.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(envelope?.Error.Code, Is.EqualTo(MediaErrorCodes.DirectUploadIncomplete));
            Assert.That(body, Does.Not.Contain("internal/object"));
        });
    }

    [TestCase(true, HttpStatusCode.Conflict, MediaErrorCodes.DirectUploadIncomplete)]
    [TestCase(false, HttpStatusCode.ServiceUnavailable, ApiErrorCodes.StorageUnavailable)]
    public async Task PostCompleteStorageFailureReturnsSafeMappedEnvelope(
        bool missing,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        var mediaId = await CreateIntentAsync();
        storage.MetadataException = missing
            ? new StorageObjectNotFoundException("missing")
            : new StorageOperationException("unavailable", new InvalidOperationException());
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.Media.UploadCompleteServicePath(mediaId),
            new CompleteDirectUploadRequest(ActorId.ToString("D"), "STUDENT"),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(expectedStatus));
            Assert.That(envelope?.Error.Code, Is.EqualTo(expectedCode));
        });
    }

    private async Task<Guid> CreateIntentAsync()
    {
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.BuildServicePath(ApiRoutes.Media.UploadIntents),
            new CreateUploadIntentRequest(
                "photo.png", "IMAGE", "image/png", 3, new string('a', 64),
                ActorId.ToString("D"), "STUDENT"),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<CreateUploadIntentResponse>>(
            TestContext.CurrentContext.CancellationToken);
        return envelope!.Data.MediaId;
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class AcceptActor : IActorValidationService
    {
        public Task<ActorReference> ValidateAsync(ActorReference actor, CancellationToken token) =>
            Task.FromResult(actor.Normalize());
    }

    private sealed class FixedAllocator : IStorageLocationAllocator
    {
        private int sequence;

        public StorageObjectLocation Allocate(StorageMediaCategory category, string extension) =>
            new("images", $"internal/object{++sequence}{extension}");
    }

    private sealed class FixedPolicy : IStorageUploadPolicyProvider
    {
        public Task<StorageUploadPolicy> CreateAsync(
            StorageUploadPolicyRequest request, CancellationToken token) =>
            Task.FromResult(new StorageUploadPolicy(
                new Uri("http://browser-minio.test/images"),
                new Dictionary<string, string> { ["policy"] = "sensitive" },
                DateTime.UnixEpoch.AddMinutes(15)));
    }

    private sealed class FixedStorage : IStorage
    {
        public long Size { get; set; } = 3;

        public Exception? MetadataException { get; set; }

        public Task<StorageObjectInfo> GetMetadataAsync(StorageObjectLocation location, CancellationToken token)
        {
            if (MetadataException is not null)
            {
                throw MetadataException;
            }

            return Task.FromResult(new StorageObjectInfo(
                location.Bucket, location.ObjectKey, "image/png", Size, "etag", null,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["checksum-sha256"] = new string('a', 64)
                }));
        }
        public Task<StorageObjectInfo> PromoteAsync(StoragePromotionRequest request, CancellationToken token) =>
            GetMetadataAsync(request.Destination, token);
        public Task<StorageObjectInfo> UploadAsync(StorageUploadRequest request, CancellationToken token) =>
            throw new NotSupportedException();
        public Task<StorageObjectInfo> DownloadAsync(StorageDownloadRequest request, CancellationToken token) =>
            throw new NotSupportedException();
        public Task<bool> ExistsAsync(StorageObjectLocation location, CancellationToken token) =>
            throw new NotSupportedException();
        public Task DeleteAsync(StorageObjectLocation location, CancellationToken token) =>
            Task.CompletedTask;
    }

    private sealed class FixedFinalizer(DirectRepository repository) : IMediaUploadFinalizer
    {
        public Task<MediaUploadFinalizationResult> FinalizeAsync(
            MediaUploadFinalizationRequest request, CancellationToken token)
        {
            var existing = repository.Media!;
            if (existing.Status == MediaObjectStatuses.Ready)
            {
                return Task.FromResult(new MediaUploadFinalizationResult(
                    existing.DraftedAtUtc!.Value, "QUEUED", null, null));
            }

            repository.Media = existing with
            {
                Status = MediaObjectStatuses.Ready,
                DraftedAtUtc = request.CompletedAtUtc,
                Location = request.FinalLocation ?? existing.Location,
            };
            return Task.FromResult(new MediaUploadFinalizationResult(
                request.CompletedAtUtc, request.Thumbnail is null ? "NOT_REQUIRED" : "QUEUED",
                request.Thumbnail?.MediaId, request.Thumbnail?.JobId));
        }
    }

    private sealed class DirectRepository : IMediaRepository
    {
        public MediaRecord? Media { get; set; }

        public Task AddPendingAsync(PendingMediaRecord media, CancellationToken token)
        {
            Media = new MediaRecord(
                media.Id, media.Location, media.MediaType, media.ContentType,
                media.OriginalFileName, media.SizeBytes, MediaObjectStatuses.Pending,
                DateTime.UnixEpoch, null, IsDraft: true, DraftedAtUtc: null,
                ChecksumSha256: media.ExpectedChecksumSha256,
                UploadedBy: media.UploadedBy);
            return Task.CompletedTask;
        }
        public Task<MediaRecord?> GetByIdAsync(Guid id, CancellationToken token) =>
            Task.FromResult(Media?.Id == id ? Media : null);
        public Task MarkFailedAsync(Guid id, string reason, CancellationToken token) => throw new NotSupportedException();
        public Task MarkReadyAsync(Guid id, string checksum, DateTime at, CancellationToken token) => throw new NotSupportedException();
        public Task EnsureMediaUsagesAsync(IReadOnlyList<CreateMediaUsageRecord> usages, CancellationToken token) => throw new NotSupportedException();
        public Task<MediaUsageUrlRecord?> GetActiveUsageUrlByIdAsync(Guid id, CancellationToken token) => throw new NotSupportedException();
        public Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(MediaUsageOwnerQuery query, CancellationToken token) => throw new NotSupportedException();
        public Task<MediaUsageRecord> ReplaceMediaThumbnailAsync(CreateMediaUsageRecord usage, CancellationToken token) => throw new NotSupportedException();
        public Task<MediaUsageRecord> ReplaceStudentAvatarAsync(CreateMediaUsageRecord usage, CancellationToken token) => throw new NotSupportedException();
    }
}
