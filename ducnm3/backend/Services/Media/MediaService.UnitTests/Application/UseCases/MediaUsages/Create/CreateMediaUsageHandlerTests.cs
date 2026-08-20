// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/MediaUsages/Create/CreateMediaUsageHandlerTests.cs
// Mục đích: Kiểm thử handler CreateMediaUsageHandlerTests và các nhánh nghiệp vụ liên quan.

using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Application.Services.Storage;
using MediaService.Application.UseCases.MediaUsages.Create;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.MediaUsages.Create;

public sealed class CreateMediaUsageHandlerTests
{
    [TestCase(MediaTypes.Image, "image.png", "image/png")]
    [TestCase(MediaTypes.Video, "video.mp4", "video/mp4")]
    [TestCase(MediaTypes.Document, "document.pdf", "application/pdf")]
    [TestCase(MediaTypes.Audio, "audio.mp3", "audio/mpeg")]
    [TestCase(MediaTypes.Other, "archive.bin", "application/octet-stream")]
    public async Task CourseLessonAttachmentAcceptsEveryReadyOriginalMediaType(
        string mediaType,
        string fileName,
        string contentType)
    {
        var repository = CreateRepository(new MediaRecord(
            Guid.NewGuid(),
            new StorageObjectLocation("attachments", fileName),
            mediaType,
            contentType,
            fileName,
            3,
            MediaObjectStatuses.Ready,
            DateTime.UtcNow,
            null));
        var handler = CreateHandler(repository);

        var result = await handler.HandleAsync(
            new CreateMediaUsageCommand(
                repository.ExistingMedia!.Id,
                MediaOwnerServices.Course,
                MediaOwnerTypes.LessonAttachment,
                Guid.NewGuid(),
                MediaUsageTypes.Attachment,
                0,
                new ActorReference(ActorTypes.Admin, Guid.NewGuid())),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.OwnerType, Is.EqualTo(MediaOwnerTypes.LessonAttachment));
            Assert.That(result.UsageType, Is.EqualTo(MediaUsageTypes.Attachment));
            Assert.That(repository.EnsuredUsages.Single().MediaId, Is.EqualTo(repository.ExistingMedia.Id));
        });
    }

    [Test]
    public async Task CourseGalleryAcceptsReadyOriginalImage()
    {
        var repository = CreateRepository(new MediaRecord(
            Guid.NewGuid(),
            new StorageObjectLocation("images", "gallery.png"),
            MediaTypes.Image,
            "image/png",
            "gallery.png",
            3,
            MediaObjectStatuses.Ready,
            DateTime.UtcNow,
            null));
        var handler = CreateHandler(repository);

        var result = await handler.HandleAsync(
            new CreateMediaUsageCommand(
                repository.ExistingMedia!.Id,
                MediaOwnerServices.Course,
                MediaOwnerTypes.CourseGallery,
                Guid.NewGuid(),
                MediaUsageTypes.Attachment,
                2,
                new ActorReference(ActorTypes.Admin, Guid.NewGuid())),
            CancellationToken.None);

        Assert.That(result.OwnerType, Is.EqualTo(MediaOwnerTypes.CourseGallery));
        Assert.That(result.UsageType, Is.EqualTo(MediaUsageTypes.Attachment));
    }

    [Test]
    public async Task CourseThumbnailUsesExclusiveUsageContract()
    {
        var repository = CreateRepository(new MediaRecord(
            Guid.NewGuid(),
            new StorageObjectLocation("images", "thumbnail.webp"),
            MediaTypes.Image,
            "image/webp",
            "thumbnail.webp",
            3,
            MediaObjectStatuses.Ready,
            DateTime.UtcNow,
            null,
            Guid.NewGuid(),
            MediaDerivationTypes.Thumbnail));
        var handler = CreateHandler(repository);

        var result = await handler.HandleAsync(
            new CreateMediaUsageCommand(
                repository.ExistingMedia!.Id,
                MediaOwnerServices.Course,
                MediaOwnerTypes.CourseThumbnail,
                Guid.NewGuid(),
                MediaUsageTypes.Thumbnail,
                0,
                new ActorReference(ActorTypes.Admin, Guid.NewGuid())),
            CancellationToken.None);

        Assert.That(result.OwnerType, Is.EqualTo(MediaOwnerTypes.CourseThumbnail));
    }

    [Test]
    public async Task UsageKeepsCreatorActorSeparateFromOwner()
    {
        var repository = new StubMediaRepository
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
        var actorValidation = new RecordingActorValidationService();
        var studentLookup = new StubStudentLookup();
        var handler = new CreateMediaUsageHandler(
            actorValidation,
            studentLookup,
            repository,
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

    private sealed class RecordingActorValidationService : IActorValidationService
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

    private static StubMediaRepository CreateRepository(MediaRecord media) => new() { ExistingMedia = media };

    private static CreateMediaUsageHandler CreateHandler(StubMediaRepository repository) =>
        new(new RecordingActorValidationService(), new StubStudentLookup(), repository, repository);
}
