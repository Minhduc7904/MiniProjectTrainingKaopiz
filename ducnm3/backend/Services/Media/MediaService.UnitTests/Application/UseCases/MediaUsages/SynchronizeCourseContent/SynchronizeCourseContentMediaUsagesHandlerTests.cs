using MediaService.Application.UseCases.MediaUsages.SynchronizeCourseContent;
using MediaService.Application.Repositories;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Constants;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.MediaUsages.SynchronizeCourseContent;

public sealed class SynchronizeCourseContentMediaUsagesHandlerTests
{
    [Test]
    public async Task HandleAsyncMixedDiffEnsuresAddedAndRemovesRequestedOwner()
    {
        var ownerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var adminId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var addedMediaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var removedMediaId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var repository = new StubMediaRepository();
        var sut = new SynchronizeCourseContentMediaUsagesHandler(repository);

        await sut.HandleAsync(
            new SynchronizeCourseContentMediaUsageV1(
                ownerId,
                MediaOwnerTypes.LessonContent,
                adminId,
                [new NotificationMediaUsageReferenceV1(addedMediaId, NotificationMediaUsageTypes.Embed, 0)],
                [new NotificationMediaUsageReferenceV1(removedMediaId, NotificationMediaUsageTypes.Attachment, 0)]),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.EnsuredUsages.Single().MediaId, Is.EqualTo(addedMediaId));
            Assert.That(repository.RemovedCourseContentUsages, Is.EqualTo([
                new CourseContentMediaUsageRemoval(
                    ownerId,
                    MediaOwnerTypes.LessonContent,
                    removedMediaId,
                    NotificationMediaUsageTypes.Attachment),
            ]));
        });
    }
}
