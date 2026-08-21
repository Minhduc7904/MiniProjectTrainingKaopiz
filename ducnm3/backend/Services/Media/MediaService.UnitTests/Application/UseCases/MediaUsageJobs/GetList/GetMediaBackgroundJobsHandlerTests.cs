// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/MediaUsageJobs/GetList/GetMediaBackgroundJobsHandlerTests.cs
// Mục đích: Kiểm thử validate filter, quyền ADMIN và pagination của use case list Media background job.

#pragma warning disable CA1707

using MediaService.Application.Repositories;
using MediaService.Application.UseCases.MediaUsageJobs.GetList;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.MediaUsageJobs.GetList;

public sealed class GetMediaBackgroundJobsHandlerTests
{
    [Test]
    public async Task HandleAsync_AdminWithNormalizedFilters_ReturnsOffsetPage()
    {
        var repository = new StubRepository(new MediaBackgroundJobListPage([], 41));
        var sut = new GetMediaBackgroundJobsHandler(repository, new StubActorValidationService());
        var correlationId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var result = await sut.HandleAsync(
            new GetMediaBackgroundJobsQuery(" notification_usage ", " processing ", correlationId.ToString(), 2, 20,
                new ActorReference(ActorTypes.Admin, Guid.Parse("22222222-2222-2222-2222-222222222222"))),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(repository.Request?.JobType, Is.EqualTo(MediaBackgroundJobTypes.NotificationUsage));
            Assert.That(repository.Request?.Status, Is.EqualTo(MediaBackgroundJobStatuses.Processing));
            Assert.That(repository.Request?.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(result.TotalPages, Is.EqualTo(3));
        });
    }

    [Test]
    public void HandleAsync_UnsupportedStatus_ThrowsSafeQueryError()
    {
        var sut = new GetMediaBackgroundJobsHandler(new StubRepository(new MediaBackgroundJobListPage([], 0)), new StubActorValidationService());

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() => sut.HandleAsync(
            new GetMediaBackgroundJobsQuery(null, "unknown", null, 1, 20,
                new ActorReference(ActorTypes.Admin, Guid.NewGuid())), CancellationToken.None));

        Assert.That(exception?.ErrorCode, Is.EqualTo("INVALID_MEDIA_JOB_QUERY"));
    }

    private sealed class StubRepository(MediaBackgroundJobListPage result) : IMediaBackgroundJobListRepository
    {
        public MediaBackgroundJobListRequest? Request { get; private set; }
        public Task<MediaBackgroundJobListPage> ListAsync(MediaBackgroundJobListRequest request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(result);
        }
    }
}
