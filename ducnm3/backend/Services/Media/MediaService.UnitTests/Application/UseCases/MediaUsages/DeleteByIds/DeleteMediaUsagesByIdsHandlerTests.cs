using MediaService.Application.UseCases.MediaUsages.DeleteByIds;
using MediaService.Contracts.Messaging;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.MediaUsages.DeleteByIds;

public sealed class DeleteMediaUsagesByIdsHandlerTests
{
    [Test]
    public async Task HandleAsync_UsageIdsProvided_DelegatesBatchCleanupToRepository()
    {
        var firstUsageId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondUsageId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var repository = new StubMediaRepository();
        var handler = new DeleteMediaUsagesByIdsHandler(repository);

        await handler.HandleAsync(
            new DeleteMediaUsagesByIdsV1([firstUsageId, secondUsageId]),
            CancellationToken.None);

        Assert.That(repository.RemovedUsageIds, Is.EqualTo([firstUsageId, secondUsageId]));
    }
}
