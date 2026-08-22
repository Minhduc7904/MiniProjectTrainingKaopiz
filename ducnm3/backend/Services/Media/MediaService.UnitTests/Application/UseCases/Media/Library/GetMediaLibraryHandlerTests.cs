// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/Media/Library/GetMediaLibraryHandlerTests.cs
// Mục đích: Bảo vệ Media Library chỉ công khai media đã hoàn tất upload.

using MediaService.Application.Repositories;
using MediaService.Application.UseCases.Media.Library;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.Media.Library;

public sealed class GetMediaLibraryHandlerTests
{
    [Test]
    public async Task HandleAsync_StatusIsLowercase_NormalizesAndPassesFilterToRepository()
    {
        var pending = CreateLibraryRecord(MediaObjectStatuses.Pending);
        var repository = new StubMediaRepository
        {
            LibraryRecords = [pending],
        };
        var handler = new GetMediaLibraryHandler(repository);

        var result = await handler.HandleAsync(
            new GetMediaLibraryQuery(
                MediaTypes.Image,
                "pending",
                null,
                20,
                new ActorReference(ActorTypes.Admin, Guid.NewGuid())),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(repository.LastLibraryStatus, Is.EqualTo(MediaObjectStatuses.Pending));
            Assert.That(result.Items, Is.EqualTo([pending]));
        });
    }

    private static MediaLibraryRecord CreateLibraryRecord(string status) =>
        new(
            Guid.NewGuid(),
            MediaTypes.Image,
            "image/png",
            "lesson.png",
            1,
            status,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);
}
