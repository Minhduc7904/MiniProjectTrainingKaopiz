// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/Media/GetContent/GetMediaContentHandlerTests.cs
// Mục đích: Kiểm thử handler GetMediaContentHandlerTests và các nhánh nghiệp vụ liên quan.

using BuildingBlocks.Contracts.Api;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Application.UseCases.Media.GetContent;
using MediaService.Domain.Constants;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.Media.GetContent;

public class GetMediaContentHandlerTests
{
    [Test]
    public async Task ReadyMediaStreamsWithoutExposingStorageLocation()
    {
        var media = CreateMedia(MediaObjectStatuses.Ready);
        var repository = new StubMediaRepository { ExistingMedia = media };
        var storage = new StubStorage();
        var handler = new GetMediaContentHandler(repository, storage);

        var result = await handler.HandleAsync(
            new GetMediaContentQuery(media.Id),
            CancellationToken.None);
        await using var destination = new MemoryStream();
        await result.CopyToAsync(destination, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(media.Id));
            Assert.That(result.ContentType, Is.EqualTo(media.ContentType));
            Assert.That(destination.ToArray(), Is.EqualTo(storage.Content));
            Assert.That(
                result.GetType().GetProperties().Select(property => property.Name),
                Does.Not.Contain("Location"));
        });
    }

    [Test]
    public void MissingMediaReturnsNotFound()
    {
        var handler = new GetMediaContentHandler(
            new StubMediaRepository(),
            new StubStorage());

        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => handler.HandleAsync(
                new GetMediaContentQuery(Guid.NewGuid()),
                CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void PendingMediaReturnsConflict()
    {
        var media = CreateMedia(MediaObjectStatuses.Pending);
        var handler = new GetMediaContentHandler(
            new StubMediaRepository { ExistingMedia = media },
            new StubStorage());

        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => handler.HandleAsync(
                new GetMediaContentQuery(media.Id),
                CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(409));
            Assert.That(
                exception.ErrorCode,
                Is.EqualTo(MediaErrorCodes.MediaNotReady));
        });
    }

    [Test]
    public async Task StorageFailureReturnsServiceUnavailable()
    {
        var media = CreateMedia(MediaObjectStatuses.Ready);
        var handler = new GetMediaContentHandler(
            new StubMediaRepository { ExistingMedia = media },
            new StubStorage { FailDownload = true });
        var result = await handler.HandleAsync(
            new GetMediaContentQuery(media.Id),
            CancellationToken.None);
        await using var destination = new MemoryStream();

        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => result.CopyToAsync(destination, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(503));
            Assert.That(
                exception.ErrorCode,
                Is.EqualTo(ApiErrorCodes.StorageUnavailable));
        });
    }

    private static MediaRecord CreateMedia(string status) =>
        new(
            Guid.NewGuid(),
            new StorageObjectLocation(
                "images",
                "2026/08/13/private-object.png"),
            MediaTypes.Image,
            "image/png",
            "avatar.png",
            3,
            status,
            DateTime.UtcNow,
            null);
}
