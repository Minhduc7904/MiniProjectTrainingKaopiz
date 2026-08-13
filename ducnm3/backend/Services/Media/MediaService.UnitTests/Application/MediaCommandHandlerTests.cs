using MediaService.Application;
using MediaService.Application.Actors;
using MediaService.Application.Persistence;
using MediaService.Application.Storage;
using MediaService.Application.Upload;
using MediaService.Application.Usages;
using MediaService.Domain;
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
        var repository = new FakeMediaRepository(events);
        var storage = new FakeStorage(events);
        var handler = CreateUploadHandler(repository, storage);
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await handler.HandleAsync(
            CreateUploadCommand(content),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.EqualTo(SuccessfulUploadEvents));
            Assert.That(result.Status, Is.EqualTo(MediaObjectStatuses.Ready));
            Assert.That(repository.Pending!.UploadedBy.Type, Is.EqualTo(ActorTypes.Student));
            Assert.That(result.ContentUrl, Does.Contain(result.Id.ToString()));
        });
    }

    [Test]
    public void UploadFailureDeletesObjectAndMarksRecordFailed()
    {
        var events = new List<string>();
        var repository = new FakeMediaRepository(events);
        var storage = new FakeStorage(events) { FailUpload = true };
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
        var repository = new FakeMediaRepository(events)
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
        var studentLookup = new FakeStudentLookup();
        var handler = new CreateMediaUsageHandler(
            actorValidation,
            studentLookup,
            repository);

        var result = await handler.HandleAsync(
            new CreateMediaUsageCommand(
                repository.ExistingMedia!.Id,
                MediaOwnerServices.Student,
                MediaOwnerTypes.StudentAvatar,
                ownerId,
                MediaUsageTypes.Avatar,
                0,
                new ActorReference(ActorTypes.Student, actorId)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(actorValidation.ValidatedActor!.Id, Is.EqualTo(actorId));
            Assert.That(studentLookup.LastStudentId, Is.EqualTo(ownerId));
            Assert.That(repository.CreatedUsage!.OwnerId, Is.EqualTo(ownerId));
            Assert.That(repository.CreatedUsage.CreatedBy.Id, Is.EqualTo(actorId));
            Assert.That(result.OwnerType, Is.EqualTo(MediaOwnerTypes.StudentAvatar));
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
        IMediaRepository repository,
        IStorage storage) =>
        new(
            new FakeActorValidationService(),
            new FakeLocationAllocator(),
            storage,
            repository,
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

    private sealed class FakeStudentLookup : IStudentLookup
    {
        public Guid LastStudentId { get; private set; }

        public Task<StudentLookupResult?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken)
        {
            LastStudentId = studentId;
            return Task.FromResult<StudentLookupResult?>(
                new StudentLookupResult(
                    studentId,
                    "student@example.com",
                    "Student",
                    "ACTIVE"));
        }
    }

    private sealed class FakeLocationAllocator : IStorageLocationAllocator
    {
        public StorageObjectLocation Allocate(
            StorageMediaCategory category,
            string extension) =>
            new("images", "2026/08/13/file.png");
    }

    private sealed class FakeStorage(List<string> events) : IStorage
    {
        public bool FailUpload { get; init; }

        public Task<StorageObjectInfo> UploadAsync(
            StorageUploadRequest request,
            CancellationToken cancellationToken)
        {
            events.Add("upload");
            if (FailUpload)
            {
                throw new StorageOperationException(
                    "Expected test failure.",
                    new InvalidOperationException());
            }

            return Task.FromResult(
                new StorageObjectInfo(
                    request.Location.Bucket,
                    request.Location.ObjectKey,
                    request.ContentType,
                    request.Size,
                    "etag",
                    new string('a', 64)));
        }

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
            throw new NotSupportedException();

        public Task DeleteAsync(
            StorageObjectLocation location,
            CancellationToken cancellationToken)
        {
            events.Add("delete");
            return Task.CompletedTask;
        }
    }

    private sealed class FakeMediaRepository(List<string> events) : IMediaRepository
    {
        public PendingMediaRecord? Pending { get; private set; }

        public MediaRecord? ExistingMedia { get; init; }

        public CreateMediaUsageRecord? CreatedUsage { get; private set; }

        public Task AddPendingAsync(
            PendingMediaRecord media,
            CancellationToken cancellationToken)
        {
            events.Add("pending");
            Pending = media;
            return Task.CompletedTask;
        }

        public Task MarkReadyAsync(
            Guid mediaId,
            string checksumSha256,
            DateTime completedAtUtc,
            CancellationToken cancellationToken)
        {
            events.Add("ready");
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(
            Guid mediaId,
            string failureReason,
            CancellationToken cancellationToken)
        {
            events.Add("failed");
            return Task.CompletedTask;
        }

        public Task<MediaRecord?> GetByIdAsync(
            Guid mediaId,
            CancellationToken cancellationToken) =>
            Task.FromResult(ExistingMedia);

        public Task<MediaUsageRecord> ReplaceStudentAvatarAsync(
            CreateMediaUsageRecord usage,
            CancellationToken cancellationToken)
        {
            CreatedUsage = usage;
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
