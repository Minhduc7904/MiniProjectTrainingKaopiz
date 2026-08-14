using MediaService.Application;
using MediaService.Application.Features.Usages.RegisterNotification;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Actors;
using MediaService.Domain.Usages;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application;

public sealed class RegisterNotificationMediaUsagesHandlerTests
{
    [Test]
    public async Task HandleAsyncBatchMapsOneReferenceToEveryNotification()
    {
        var firstNotificationId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondNotificationId =
            Guid.Parse("22222222-2222-2222-2222-222222222222");
        var mediaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var createdBy = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var repository = new StubMediaRepository();
        var sut = new RegisterNotificationMediaUsagesHandler(repository);

        await sut.HandleAsync(
            new RegisterNotificationMediaUsageBatchV1(
                [firstNotificationId, secondNotificationId],
                createdBy,
                [
                    new NotificationMediaUsageReferenceV1(
                        mediaId,
                        NotificationMediaUsageTypes.Embed,
                        0),
                ]),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.EnsuredUsages, Has.Count.EqualTo(2));
            Assert.That(
                repository.EnsuredUsages.Select(usage => usage.OwnerId),
                Is.EquivalentTo([firstNotificationId, secondNotificationId]));
            Assert.That(
                repository.EnsuredUsages.All(usage =>
                    usage.MediaId == mediaId &&
                    usage.OwnerService == MediaOwnerServices.Notification &&
                    usage.OwnerType == MediaOwnerTypes.NotificationBody &&
                    usage.UsageType == NotificationMediaUsageTypes.Embed &&
                    usage.CreatedBy == new ActorReference(ActorTypes.Admin, createdBy)),
                Is.True);
        });
    }

    [Test]
    public void HandleAsyncBatchExceedsUsageRowLimitThrowsValidationError()
    {
        var notificationIds = Enumerable.Range(
                0,
                NotificationMediaUsageBatchLimits.MaxNotificationIdsPerCommand)
            .Select(_ => Guid.NewGuid())
            .ToArray();
        var sut = new RegisterNotificationMediaUsagesHandler(
            new StubMediaRepository());

        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => sut.HandleAsync(
                new RegisterNotificationMediaUsageBatchV1(
                    notificationIds,
                    Guid.NewGuid(),
                    [
                        new NotificationMediaUsageReferenceV1(
                            Guid.NewGuid(),
                            NotificationMediaUsageTypes.Embed,
                            0),
                        new NotificationMediaUsageReferenceV1(
                            Guid.NewGuid(),
                            NotificationMediaUsageTypes.Attachment,
                            1),
                        new NotificationMediaUsageReferenceV1(
                            Guid.NewGuid(),
                            NotificationMediaUsageTypes.Embed,
                            2),
                    ]),
                CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(MediaErrorCodes.InvalidMedia));
    }
}
