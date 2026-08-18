// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/MediaUsages/GetUrls/GetMediaUsageUrlsHandlerTests.cs
// Mục đích: Kiểm thử handler GetMediaUsageUrlsHandlerTests và các nhánh nghiệp vụ liên quan.

using MediaService.Application.Services.Urls;
using MediaService.Application.UseCases.MediaUsages.GetUrls;
using MediaService.Domain.Constants;
using MediaService.UnitTests.TestDoubles;

using MediaService.Application.Common.Errors;

namespace MediaService.UnitTests.Application.UseCases.MediaUsages.GetUrls;

public sealed class GetMediaUsageUrlsHandlerTests
{
    [Test]
    public async Task HandleAsyncOwnerHasImageUsagesReturnsUrlForEveryUsage()
    {
        // Arrange
        var first = CreateRecord(Guid.Parse("0e7f8a70-5cc7-43f9-a8bf-86cd7bec3e85"), 0);
        var second = CreateRecord(Guid.Parse("14c30ef8-fb39-459e-8b0a-3875c3da813b"), 1);
        var repository = new StubMediaRepository { UsageUrls = [first, second] };
        var sut = new GetMediaUsageUrlsHandler(
            repository,
            new StubMediaUrlProvider());

        // Act
        var result = await sut.HandleAsync(
            new GetMediaUsageUrlsQuery(
                "student",
                "student_avatar",
                "avatar",
                Guid.Parse("fe4f42ef-346e-4c76-8474-46670d9ecc10")),
            TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(item => item.DisplayOrder), Is.EqualTo(new uint[] { 0, 1 }));
            Assert.That(result.All(item => item.Url == "/media/api/media/content"), Is.True);
            Assert.That(repository.LastUsageUrlQuery?.UsageType, Is.EqualTo("AVATAR"));
        });
    }

    [Test]
    public void HandleAsyncOwnerIdEmptyThrowsValidationError()
    {
        // Arrange
        var sut = new GetMediaUsageUrlsHandler(
            new StubMediaRepository(),
            new StubMediaUrlProvider());

        // Act
        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(
            () => sut.HandleAsync(
                new GetMediaUsageUrlsQuery("STUDENT", "STUDENT_AVATAR", "AVATAR", Guid.Empty),
                TestContext.CurrentContext.CancellationToken));

        // Assert
        Assert.That(exception!.ErrorCode, Is.EqualTo(MediaService.Application.Common.Errors.MediaErrorCodes.InvalidMedia));
    }

    private static MediaService.Application.Repositories.MediaUsageUrlRecord CreateRecord(
        Guid usageId,
        uint displayOrder) =>
        new(
            new(
                usageId,
                Guid.Parse("942ae599-96da-4bc2-b1ac-9d4eea1bff80"),
                "STUDENT",
                "STUDENT_AVATAR",
                Guid.Parse("fe4f42ef-346e-4c76-8474-46670d9ecc10"),
                "AVATAR",
                displayOrder,
                DateTime.UnixEpoch),
            new(
                Guid.Parse("942ae599-96da-4bc2-b1ac-9d4eea1bff80"),
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
            MediaService.Application.Repositories.MediaRecord media,
            CancellationToken cancellationToken) =>
            Task.FromResult(new MediaUrl("/media/api/media/content", null));

        public Task<IReadOnlyList<MediaUrl>> GenerateManyAsync(
            IReadOnlyList<MediaService.Application.Repositories.MediaRecord> media,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MediaUrl>>(
                media.Select(_ => new MediaUrl("/media/api/media/content", null)).ToArray());
    }
}
