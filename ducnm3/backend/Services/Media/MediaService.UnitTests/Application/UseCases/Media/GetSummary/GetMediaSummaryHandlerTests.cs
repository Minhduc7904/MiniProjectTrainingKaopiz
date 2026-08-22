// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/Media/GetSummary/GetMediaSummaryHandlerTests.cs
// Mục đích: Kiểm thử mapping tổng số media của use case dashboard.

using MediaService.Application.Repositories;
using MediaService.Application.UseCases.Media.GetSummary;

namespace MediaService.UnitTests.Application.UseCases.Media.GetSummary;

public sealed class GetMediaSummaryHandlerTests
{
    [Test]
    public async Task HandleAsync_RepositoryReturnsCount_MapsTotalMedia()
    {
        var handler = new GetMediaSummaryHandler(new Repository(9));

        var result = await handler.HandleAsync(TestContext.CurrentContext.CancellationToken);

        Assert.That(result.TotalMedia, Is.EqualTo(9));
    }

    private sealed class Repository(long count) : IMediaSummaryRepository
    {
        public Task<long> CountAsync(CancellationToken cancellationToken) => Task.FromResult(count);
    }
}
