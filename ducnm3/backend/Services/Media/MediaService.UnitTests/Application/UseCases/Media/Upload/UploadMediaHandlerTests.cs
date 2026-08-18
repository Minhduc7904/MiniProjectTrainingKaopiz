// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/Media/Upload/UploadMediaHandlerTests.cs
// Mục đích: Kiểm thử handler UploadMediaHandlerTests và các nhánh nghiệp vụ liên quan.

using MediaService.Application;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.Services.Actors;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Domain.ValueObjects;
using MediaService.Domain.Constants;
using MediaService.UnitTests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

using MediaService.Application.Common.Errors;

namespace MediaService.UnitTests.Application.UseCases.Media.Upload;

public class UploadMediaHandlerTests
{
    private static readonly DateTime UploadCompletedAtUtc =
        new(2026, 8, 18, 3, 4, 5, 678, DateTimeKind.Utc);
    private static readonly string[] SuccessfulUploadEvents =
        ["pending", "upload", "ready"];
    private static readonly string[] FailedUploadEvents =
        ["pending", "upload", "delete", "failed"];

    [Test]
    public async Task UploadCreatesPendingBeforeStorageAndThenMarksReady()
    {
        var events = new List<string>();
        var repository = new StubMediaRepository(events);
        var storage = new StubStorage(events);
        var handler = CreateUploadHandler(repository, storage);
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await handler.HandleAsync(
            CreateUploadCommand(content),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.EqualTo(SuccessfulUploadEvents));
            Assert.That(result.Status, Is.EqualTo(MediaObjectStatuses.Ready));
            Assert.That(result.IsDraft, Is.True);
            Assert.That(result.DraftedAtUtc, Is.EqualTo(UploadCompletedAtUtc));
            Assert.That(result.ThumbnailStatus, Is.EqualTo("QUEUED"));
            Assert.That(result.ThumbnailMediaId, Is.Not.Null);
            Assert.That(result.ThumbnailJobId, Is.Not.Null);
            Assert.That(repository.Pending!.UploadedBy.Type, Is.EqualTo(ActorTypes.Student));
            Assert.That(repository.Pending.IsDraft, Is.True);
            Assert.That(repository.Pending.DraftedAtUtc, Is.Null);
        });
    }

    [Test]
    public void UploadFailureDeletesObjectAndMarksRecordFailed()
    {
        var events = new List<string>();
        var repository = new StubMediaRepository(events);
        var storage = new StubStorage(events) { FailUpload = true };
        var handler = CreateUploadHandler(repository, storage);
        using var content = new MemoryStream([1, 2, 3]);

        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => handler.HandleAsync(
                CreateUploadCommand(content),
                CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo(MediaErrorCodes.MediaUploadFailed));
            Assert.That(events, Is.EqualTo(FailedUploadEvents));
        });
    }

    [Test]
    public async Task ActorValidationDispatchesWithoutChangingHandlers()
    {
        var studentValidator = new RecordingActorValidator(ActorTypes.Student);
        var adminValidator = new RecordingActorValidator("ADMIN");
        var service = new ActorValidationService(
            [studentValidator, adminValidator]);
        var admin = new ActorReference("admin", Guid.NewGuid());

        var result = await service.ValidateAsync(admin, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Type, Is.EqualTo("ADMIN"));
            Assert.That(adminValidator.WasCalled, Is.True);
            Assert.That(studentValidator.WasCalled, Is.False);
        });
    }

    private static UploadMediaHandler CreateUploadHandler(
        StubMediaRepository repository,
        IStorage storage) =>
        new(
            new FakeActorValidationService(),
            new FakeLocationAllocator(),
            storage,
            repository,
            new FakeUploadFinalizer(repository.Events),
            new MediaUploadOptions(),
            new FixedTimeProvider(UploadCompletedAtUtc),
            NullLogger<UploadMediaHandler>.Instance);

    private static UploadMediaCommand CreateUploadCommand(Stream content) =>
        new(
            MediaTypes.Image,
            "image/png",
            "avatar.png",
            content,
            content.Length,
            new ActorReference("student", Guid.NewGuid()));

    private sealed class FakeActorValidationService : IActorValidationService
    {
        public ActorReference? ValidatedActor { get; private set; }

        public Task<ActorReference> ValidateAsync(
            ActorReference actor,
            CancellationToken cancellationToken)
        {
            var normalizedActor = actor.Normalize();
            ValidatedActor = normalizedActor;
            return Task.FromResult(normalizedActor);
        }
    }

    private sealed class RecordingActorValidator(string actorType)
        : IActorValidator
    {
        public string ActorType => actorType;

        public bool WasCalled { get; private set; }

        public Task ValidateAsync(
            ActorReference actor,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeLocationAllocator : IStorageLocationAllocator
    {
        public StorageObjectLocation Allocate(
            StorageMediaCategory category,
            string extension) =>
            new("images", "2026/08/13/file.png");
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(utcNow, TimeSpan.Zero);
    }

    private sealed class FakeUploadFinalizer(List<string> events)
        : IMediaUploadFinalizer
    {
        public Task<MediaUploadFinalizationResult> FinalizeAsync(
            MediaUploadFinalizationRequest request,
            CancellationToken cancellationToken)
        {
            events.Add("ready");
            return Task.FromResult(
                new MediaUploadFinalizationResult(
                    request.CompletedAtUtc,
                    request.Thumbnail is null ? "NOT_REQUIRED" : "QUEUED",
                    request.Thumbnail?.MediaId,
                    request.Thumbnail?.JobId));
        }
    }
}
