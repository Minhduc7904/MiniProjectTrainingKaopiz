using MediaService.Application.UseCases.MediaUsages.SynchronizeMarkdown;
using MediaService.Application.Repositories;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Constants;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.MediaUsages.SynchronizeMarkdown;

public sealed class SynchronizeMarkdownMediaUsagesHandlerTests
{
    [Test]
    public async Task HandleAsyncMixedDiffEnsuresAddedAndRemovesRequestedOwner()
    {
        var ownerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var adminId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var addedMediaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var removedMediaId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var repository = new StubMediaRepository();
        var sut = new SynchronizeMarkdownMediaUsagesHandler(repository);

        await sut.HandleAsync(
            new SynchronizeMarkdownMediaUsageV1(
                MarkdownMediaUsageOwnerServices.Course,
                MarkdownMediaUsageOwnerTypes.LessonContent,
                ownerId,
                adminId,
                [new MarkdownMediaUsageReferenceV1(addedMediaId, MarkdownMediaUsageTypes.Embed, 0)],
                [new MarkdownMediaUsageReferenceV1(removedMediaId, MarkdownMediaUsageTypes.Attachment, 0)]),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.EnsuredUsages.Single().MediaId, Is.EqualTo(addedMediaId));
            Assert.That(repository.RemovedCourseContentUsages, Is.EqualTo([
                new CourseContentMediaUsageRemoval(
                    ownerId,
                MarkdownMediaUsageOwnerTypes.LessonContent,
                removedMediaId,
                MarkdownMediaUsageTypes.Attachment),
            ]));
        });
    }
}
