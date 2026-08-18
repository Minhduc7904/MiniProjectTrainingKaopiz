// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/MediaUsages/Create/CreateMediaUsageHandlerTests.cs
// Mục đích: Triển khai use case nghiệp vụ của Media Service.

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
}
