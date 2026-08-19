// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/MediaUsages/GetUrl/GetMediaUsageUrlHandlerTests.cs
// Mục đích: Kiểm thử handler GetMediaUsageUrlHandlerTests và các nhánh nghiệp vụ liên quan.

using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Urls;
using MediaService.Application.UseCases.MediaUsages.GetUrl;
using MediaService.Domain.Constants;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.MediaUsages.GetUrl;

public sealed class GetMediaUsageUrlHandlerTests
{
    private static readonly Guid UsageId =
        Guid.Parse("d5236209-5efb-47d7-b012-3c0fc85e0d7e");
    private static readonly Guid MediaId =
        Guid.Parse("d3e1435b-8dbe-44eb-8dc6-ae69f6873ef7");

    [Test]
    public async Task HandleAsyncActiveImageUsageReturnsGeneratedUrl()
    {
        // Arrange
        var repository = new StubMediaRepository
        {
            ExistingUsageUrl = CreateRecord(),
        };
        var sut = new GetMediaUsageUrlHandler(
            repository,
            new StubMediaUrlProvider());

        // Act
        var result = await sut.HandleAsync(
            new GetMediaUsageUrlQuery(UsageId),
            TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.UsageId, Is.EqualTo(UsageId));
            Assert.That(result.MediaId, Is.EqualTo(MediaId));
            Assert.That(result.Url, Is.EqualTo("/media/api/media/content"));
        });
    }

    [Test]
    public void HandleAsyncUsageMissingThrowsNotFound()
    {
        // Arrange
        var sut = new GetMediaUsageUrlHandler(
            new StubMediaRepository(),
            new StubMediaUrlProvider());

        // Act
        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => sut.HandleAsync(
                new GetMediaUsageUrlQuery(UsageId),
                TestContext.CurrentContext.CancellationToken));

        // Assert
        Assert.That(exception!.ErrorCode, Is.EqualTo(MediaErrorCodes.MediaUsageNotFound));
    }

    [Test]
    public async Task HandleAsyncNonImageUsageThrowsMediaNotReady()
    {
        // Arrange
        var record = CreateRecord() with
        {
            Media = CreateRecord().Media with { MediaType = MediaTypes.Document },
        };
        var sut = new GetMediaUsageUrlHandler(
            new StubMediaRepository { ExistingUsageUrl = record },
            new StubMediaUrlProvider());

        // Act
        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => sut.HandleAsync(
                new GetMediaUsageUrlQuery(UsageId),
                TestContext.CurrentContext.CancellationToken));

        // Assert
        Assert.That(exception!.ErrorCode, Is.EqualTo(MediaErrorCodes.MediaNotReady));
    }

    private static MediaUsageUrlRecord CreateRecord() =>
        new(
            new MediaUsageRecord(
                UsageId,
                MediaId,
                "STUDENT",
                "STUDENT_AVATAR",
                Guid.Parse("eab68a97-1af0-4d6f-bdd9-9fc750670afc"),
                "AVATAR",
                0,
                DateTime.UnixEpoch),
            new MediaRecord(
                MediaId,
                new("images", "2026/08/14/image.png"),
                MediaTypes.Image,
                "image/png",
                "image.png",
                20,
                MediaObjectStatuses.Ready,
                DateTime.UnixEpoch,
                null));

    private sealed class StubMediaUrlProvider : IMediaUrlProvider
    {
        public Task<MediaUrl> GenerateAsync(
            MediaRecord media,
            CancellationToken cancellationToken) =>
            Task.FromResult(new MediaUrl("/media/api/media/content", null));

        public Task<IReadOnlyList<MediaUrl>> GenerateManyAsync(
            IReadOnlyList<MediaRecord> media,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MediaUrl>>(
                media.Select(_ => new MediaUrl("/media/api/media/content", null)).ToArray());
    }
}
