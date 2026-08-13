using MediaService.Application;
using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Abstractions.Storage;
using MediaService.Application.Actors;
using MediaService.Application.Features.Media.Upload;
using MediaService.Application.Features.Usages.Create;
using MediaService.Domain.Actors;
using MediaService.Domain.Media;
using MediaService.Domain.Usages;
using MediaService.UnitTests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediaService.UnitTests.Application;

public class MediaCommandHandlerTests
{
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
            Assert.That(result.ThumbnailStatus, Is.EqualTo("QUEUED"));
            Assert.That(result.ThumbnailMediaId, Is.Not.Null);
            Assert.That(result.ThumbnailJobId, Is.Not.Null);
            Assert.That(repository.Pending!.UploadedBy.Type, Is.EqualTo(ActorTypes.Student));
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
    public async Task UsageKeepsCreatorActorSeparateFromOwner()
    {
        var events = new List<string>();
        var repository = new StubMediaRepository(events)
        {
            ExistingMedia = new MediaRecord(
                Guid.NewGuid(),
                new StorageObjectLocation("images", "2026/08/13/file.png"),
                MediaTypes.Image,
                "image/png",
                "file.png",
                3,
                MediaObjectStatuses.Ready,
                DateTime.UtcNow,
                null),
        };
        var actorId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var actorValidation = new FakeActorValidationService();
        var studentLookup = new StubStudentLookup();
        var handler = new CreateMediaUsageHandler(
            actorValidation,
            studentLookup,
            repository);

        var result = await handler.HandleAsync(
            new CreateMediaUsageCommand(
                repository.ExistingMedia!.Id,
                "student",
                "student_avatar",
                ownerId,
                "avatar",
                0,
                new ActorReference(ActorTypes.Student, actorId)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(actorValidation.ValidatedActor!.Id, Is.EqualTo(actorId));
            Assert.That(studentLookup.LastStudentId, Is.EqualTo(ownerId));
            Assert.That(repository.CreatedUsage!.OwnerId, Is.EqualTo(ownerId));
            Assert.That(repository.CreatedUsage.CreatedBy.Id, Is.EqualTo(actorId));
            Assert.That(
                repository.CreatedUsage.OwnerService,
                Is.EqualTo(MediaOwnerServices.Student));
            Assert.That(result.OwnerType, Is.EqualTo(MediaOwnerTypes.StudentAvatar));
            Assert.That(result.UsageType, Is.EqualTo(MediaUsageTypes.Avatar));
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
            TimeProvider.System,
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
                    request.Thumbnail is null ? "NOT_REQUIRED" : "QUEUED",
                    request.Thumbnail?.MediaId,
                    request.Thumbnail?.JobId));
        }
    }
}
