// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/Media/DirectUpload/Complete/CompleteDirectUploadHandlerTests.cs
// Mục đích: Kiểm thử handler CompleteDirectUploadHandlerTests và các nhánh nghiệp vụ liên quan.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.UseCases.Media.DirectUpload.Complete;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.Media.DirectUpload.Complete;

public sealed class CompleteDirectUploadHandlerTests
{
    private static readonly ActorReference Actor =
        new("STUDENT", Guid.Parse("11111111-1111-1111-1111-111111111111"));
    private static readonly StorageObjectLocation Location =
        new("images", "2026/08/file.png");
    private static readonly DateTime CompletionTime =
        new(2026, 8, 18, 4, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task HandleAsyncMetadataMatchesUsesDeclaredChecksumWithoutReadingObjectBytes()
    {
        var actor = new ActorReference("STUDENT", Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var location = new StorageObjectLocation("images", "2026/08/file.png");
        var repository = new StubMediaRepository
        {
            ExistingMedia = new MediaRecord(
                Guid.Parse("22222222-2222-2222-2222-222222222222"), location,
                "IMAGE", "image/png", "file.png", 3, MediaObjectStatuses.Pending,
                DateTime.UtcNow, null, IsDraft: true, DraftedAtUtc: null,
                ChecksumSha256: new string('a', 64), UploadedBy: actor)
        };
        var storage = new StubStorage
        {
            Metadata = new StorageObjectInfo(
                location.Bucket, location.ObjectKey, "image/png", 3, "etag", null,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["checksum-sha256"] = new string('a', 64)
                })
        };
        var finalizer = new StubDirectUploadFinalizer();
        var handler = new CompleteDirectUploadHandler(
            new StubActorValidationService(), storage, repository, finalizer,
            new StubStorageLocationAllocator(), TimeProvider.System);

        var result = await handler.HandleAsync(
            new CompleteDirectUploadCommand(repository.ExistingMedia.Id, actor),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(MediaObjectStatuses.Ready));
            Assert.That(storage.DownloadCallCount, Is.Zero);
            Assert.That(finalizer.CallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void HandleAsyncMediaMissingReturnsNotFound()
    {
        var handler = CreateHandler(new StubMediaRepository(), ValidStorage(), new StubDirectUploadFinalizer());

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                new CompleteDirectUploadCommand(Guid.NewGuid(), Actor),
                TestContext.CurrentContext.CancellationToken));

        Assert.That(exception!.ErrorCode, Is.EqualTo("MEDIA_NOT_FOUND"));
    }

    [Test]
    public void HandleAsyncOwnerMismatchReturnsNotFoundWithoutReadingStorage()
    {
        var repository = RepositoryWithMedia();
        var storage = ValidStorage();
        var handler = CreateHandler(repository, storage, new StubDirectUploadFinalizer());

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                new CompleteDirectUploadCommand(
                    repository.ExistingMedia!.Id,
                    new ActorReference("STUDENT", Guid.NewGuid())),
                TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo("MEDIA_NOT_FOUND"));
            Assert.That(storage.MetadataCallCount, Is.Zero);
        });
    }

    [Test]
    public void HandleAsyncStorageObjectOperationalFailureReturnsStorageUnavailable()
    {
        var repository = RepositoryWithMedia();
        var handler = CreateHandler(repository, new StubStorage(), new StubDirectUploadFinalizer());

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
                TestContext.CurrentContext.CancellationToken));

        Assert.That(exception!.ErrorCode, Is.EqualTo("STORAGE_UNAVAILABLE"));
    }

    [Test]
    public void HandleAsyncStorageObjectMissingReturnsSafeIncompleteError()
    {
        var repository = RepositoryWithMedia();
        var storage = new StubStorage
        {
            MetadataException = new StorageObjectNotFoundException("Missing staging object.")
        };
        var handler = CreateHandler(repository, storage, new StubDirectUploadFinalizer());

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
                TestContext.CurrentContext.CancellationToken));

        Assert.That(exception!.ErrorCode, Is.EqualTo("MEDIA_UPLOAD_INCOMPLETE"));
    }

    [Test]
    public async Task HandleAsyncPendingPromotesToUniqueFinalLocationAndDeletesStaging()
    {
        var repository = RepositoryWithMedia();
        var storage = ValidStorage();
        var finalizer = new StubDirectUploadFinalizer();
        var handler = CreateHandler(repository, storage, finalizer);

        await handler.HandleAsync(
            new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(storage.PromoteCallCount, Is.EqualTo(1));
            Assert.That(storage.LastPromotion!.Source, Is.EqualTo(Location));
            Assert.That(storage.LastPromotion.SourceETag, Is.EqualTo("etag"));
            Assert.That(finalizer.LastRequest!.FinalLocation,
                Is.EqualTo(storage.LastPromotion.Destination));
            Assert.That(finalizer.LastRequest.FinalLocation, Is.Not.EqualTo(Location));
            Assert.That(storage.DeleteCallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void HandleAsyncStagingChangesBeforePromotionDoesNotFinalize()
    {
        var repository = RepositoryWithMedia();
        var storage = ValidStorage();
        storage.PromotionException = new StorageObjectNotFoundException("ETag precondition failed.");
        var finalizer = new StubDirectUploadFinalizer();
        var handler = CreateHandler(repository, storage, finalizer);

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
                TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo("MEDIA_UPLOAD_INCOMPLETE"));
            Assert.That(finalizer.CallCount, Is.Zero);
        });
    }

    [Test]
    public async Task HandleAsyncConcurrentLoserDeletesOnlyItsAttemptedFinalObject()
    {
        var repository = RepositoryWithMedia();
        var storage = ValidStorage();
        var finalizer = new StubDirectUploadFinalizer { Transitioned = false };
        var handler = CreateHandler(repository, storage, finalizer);

        await handler.HandleAsync(
            new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
            TestContext.CurrentContext.CancellationToken);

        Assert.That(storage.DeletedLocations,
            Is.EqualTo(new[] { storage.LastPromotion!.Destination }));
    }

    [Test]
    public void HandleAsyncFinalizerThrowsPreservesAttemptedFinalObjectForReferenceCheckedCleanup()
    {
        var repository = RepositoryWithMedia();
        var storage = ValidStorage();
        var finalizer = new StubDirectUploadFinalizer
        {
            Exception = new InvalidOperationException("Expected finalizer failure.")
        };
        var handler = CreateHandler(repository, storage, finalizer);

        Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
            TestContext.CurrentContext.CancellationToken));

        Assert.That(storage.DeletedLocations, Is.Empty);
    }

    [TestCase(4L, "image/png", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [TestCase(3L, "video/mp4", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [TestCase(3L, "image/png", "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")]
    public void HandleAsyncStoredMetadataMismatchReturnsSafeIncompleteError(
        long size,
        string contentType,
        string checksum)
    {
        var repository = RepositoryWithMedia();
        var storage = ValidStorage(size, contentType, checksum);
        var finalizer = new StubDirectUploadFinalizer();
        var handler = CreateHandler(repository, storage, finalizer);

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
                TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo("MEDIA_UPLOAD_INCOMPLETE"));
            Assert.That(finalizer.CallCount, Is.Zero);
        });
    }

    [Test]
    public async Task HandleAsyncAlreadyReadySkipsStorageAndReturnsEstablishedResult()
    {
        var repository = RepositoryWithMedia(MediaObjectStatuses.Ready);
        var storage = new StubStorage();
        var finalizer = new StubDirectUploadFinalizer();
        var handler = CreateHandler(repository, storage, finalizer);

        var result = await handler.HandleAsync(
            new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(MediaObjectStatuses.Ready));
            Assert.That(storage.MetadataCallCount, Is.Zero);
            Assert.That(storage.PromoteCallCount, Is.Zero);
            Assert.That(storage.DeleteCallCount, Is.Zero);
            Assert.That(finalizer.CallCount, Is.EqualTo(1));
            Assert.That(finalizer.LastRequest!.Thumbnail, Is.Null);
        });
    }

    [TestCase("IMAGE", "image/png", true)]
    [TestCase("DOCUMENT", "text/plain", false)]
    public async Task HandleAsyncThumbnailPolicyReservesOnlyWhenRequired(
        string mediaType,
        string contentType,
        bool expectedReservation)
    {
        var repository = RepositoryWithMedia(
            MediaObjectStatuses.Pending,
            mediaType,
            contentType);
        var storage = ValidStorage(3, contentType, new string('a', 64));
        var finalizer = new StubDirectUploadFinalizer();
        var handler = CreateHandler(repository, storage, finalizer);

        var result = await handler.HandleAsync(
            new CompleteDirectUploadCommand(repository.ExistingMedia!.Id, Actor),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(finalizer.LastRequest!.Thumbnail is not null, Is.EqualTo(expectedReservation));
            Assert.That(result.IsDraft, Is.True);
            Assert.That(result.DraftedAtUtc, Is.EqualTo(CompletionTime));
        });
    }

    private static CompleteDirectUploadHandler CreateHandler(
        StubMediaRepository repository,
        StubStorage storage,
        StubDirectUploadFinalizer finalizer) =>
        new(
            new StubActorValidationService(), storage, repository, finalizer,
            new StubStorageLocationAllocator(), new FixedTimeProvider());

    private static StubMediaRepository RepositoryWithMedia(
        string status = MediaObjectStatuses.Pending,
        string mediaType = "IMAGE",
        string contentType = "image/png") =>
        new()
        {
            ExistingMedia = new MediaRecord(
                Guid.Parse("22222222-2222-2222-2222-222222222222"), Location,
                mediaType, contentType, "file.png", 3, status,
                DateTime.UnixEpoch, null, IsDraft: true, DraftedAtUtc: null,
                ChecksumSha256: new string('a', 64), UploadedBy: Actor)
        };

    private static StubStorage ValidStorage(
        long size = 3,
        string contentType = "image/png",
        string? checksum = null) =>
        new()
        {
            Metadata = new StorageObjectInfo(
                Location.Bucket, Location.ObjectKey, contentType, size, "etag", null,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["checksum-sha256"] = checksum ?? new string('a', 64)
                })
        };

    private sealed class StubDirectUploadFinalizer : IMediaUploadFinalizer
    {
        public int CallCount { get; private set; }

        public MediaUploadFinalizationRequest? LastRequest { get; private set; }

        public bool Transitioned { get; init; } = true;

        public Exception? Exception { get; init; }

        public Task<MediaUploadFinalizationResult> FinalizeAsync(
            MediaUploadFinalizationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            if (Exception is not null)
            {
                throw Exception;
            }
            return Task.FromResult(new MediaUploadFinalizationResult(
                request.CompletedAtUtc, "QUEUED", request.Thumbnail?.MediaId,
                request.Thumbnail?.JobId, Transitioned));
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(CompletionTime);
    }
}
