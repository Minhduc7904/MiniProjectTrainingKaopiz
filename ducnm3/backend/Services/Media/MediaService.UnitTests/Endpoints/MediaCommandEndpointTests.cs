using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Endpoints;
using MediaService.Application;
using MediaService.Application.Actors;
using MediaService.Application.Persistence;
using MediaService.Application.Storage;
using MediaService.Application.Upload;
using MediaService.Application.Usages;
using MediaService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediaService.UnitTests.Endpoints;

public sealed class MediaCommandEndpointTests
{
    private readonly ComponentRepository repository = new();
    private WebApplication app = null!;
    private HttpClient client = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IMediaRepository>(repository);
        builder.Services.AddSingleton<IStorage, ComponentStorage>();
        builder.Services.AddSingleton<IStorageLocationAllocator, ComponentAllocator>();
        builder.Services.AddSingleton<IStudentLookup, ExistingStudentLookup>();
        builder.Services.AddSingleton<IActorValidator, StudentActorValidator>();
        builder.Services.AddSingleton<IActorValidationService, ActorValidationService>();
        builder.Services.AddSingleton(new MediaUploadOptions());
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton(NullLogger<UploadMediaHandler>.Instance);
        builder.Services.AddSingleton<UploadMediaHandler>();
        builder.Services.AddSingleton<CreateMediaUsageHandler>();

        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapUploadMedia();
        app.MapCreateMediaUsage();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task MultipartUploadAndUsageReturnStandardCreatedResponses()
    {
        var actorId = Guid.NewGuid();
        using var multipart = CreateMultipart(actorId, ActorTypes.Student);

        using var uploadResponse = await client.PostAsync(
            "/api/media",
            multipart);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(uploadResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(uploadBody, Does.Contain("\"status\":\"READY\""));
            Assert.That(uploadBody, Does.Not.Contain("objectKey"));
            Assert.That(uploadBody, Does.Not.Contain("\"bucket\""));
        });

        var mediaId = repository.LastPending!.Id;
        var ownerId = Guid.NewGuid();
        using var usageResponse = await client.PostAsJsonAsync(
            "/api/media/usages",
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
            Assert.That(repository.LastUsage!.OwnerId, Is.EqualTo(ownerId));
            Assert.That(repository.LastUsage.CreatedBy.Id, Is.EqualTo(actorId));
        });
    }

    [Test]
    public async Task UnknownActorTypeReturnsSafeValidationError()
    {
        using var multipart = CreateMultipart(Guid.NewGuid(), "ADMIN");

        using var response = await client.PostAsync("/api/media", multipart);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body, Does.Contain(MediaErrorCodes.InvalidActorType));
            Assert.That(body, Does.Not.Contain("stack"));
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
        var file = new ByteArrayContent([1, 2, 3]);
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

    private sealed class ComponentStorage : IStorage
    {
        public Task<StorageObjectInfo> UploadAsync(
            StorageUploadRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new StorageObjectInfo(
                    request.Location.Bucket,
                    request.Location.ObjectKey,
                    request.ContentType,
                    request.Size,
                    "etag",
                    new string('a', 64)));

        public Task<StorageObjectInfo> DownloadAsync(
            StorageDownloadRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<StorageObjectInfo> GetMetadataAsync(
            StorageObjectLocation location,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> ExistsAsync(
            StorageObjectLocation location,
            CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task DeleteAsync(
            StorageObjectLocation location,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class ExistingStudentLookup : IStudentLookup
    {
        public Task<StudentLookupResult?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult<StudentLookupResult?>(
                new(studentId, "student@example.com", "Student", "ACTIVE"));
    }

    private sealed class ComponentRepository : IMediaRepository
    {
        private MediaRecord? media;

        public PendingMediaRecord? LastPending { get; private set; }

        public CreateMediaUsageRecord? LastUsage { get; private set; }

        public Task AddPendingAsync(
            PendingMediaRecord pending,
            CancellationToken cancellationToken)
        {
            LastPending = pending;
            media = new MediaRecord(
                pending.Id,
                pending.Location,
                pending.MediaType,
                pending.ContentType,
                pending.OriginalFileName,
                pending.SizeBytes,
                MediaObjectStatuses.Pending,
                DateTime.UtcNow,
                null);
            return Task.CompletedTask;
        }

        public Task MarkReadyAsync(
            Guid mediaId,
            string checksumSha256,
            DateTime completedAtUtc,
            CancellationToken cancellationToken)
        {
            media = media! with { Status = MediaObjectStatuses.Ready };
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(
            Guid mediaId,
            string failureReason,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<MediaRecord?> GetByIdAsync(
            Guid mediaId,
            CancellationToken cancellationToken) =>
            Task.FromResult(media?.Id == mediaId ? media : null);

        public Task<MediaUsageRecord> ReplaceStudentAvatarAsync(
            CreateMediaUsageRecord usage,
            CancellationToken cancellationToken)
        {
            LastUsage = usage;
            return Task.FromResult(
                new MediaUsageRecord(
                    usage.Id,
                    usage.MediaId,
                    usage.OwnerService,
                    usage.OwnerType,
                    usage.OwnerId,
                    usage.UsageType,
                    usage.DisplayOrder,
                    DateTime.UtcNow));
        }
    }
}
