// File: backend/Services/Media/MediaService.UnitTests/TestDoubles/StubStorage.cs
// Mục đích: Cung cấp test double StubStorage để unit test cô lập use case khỏi dependency bên ngoài.

using MediaService.Application.Services.Storage;

namespace MediaService.UnitTests.TestDoubles;

public sealed class StubStorage(List<string>? sharedEvents = null) : IStorage
{
    public List<string> Events { get; } = sharedEvents ?? [];

    public bool FailUpload { get; set; }

    public bool FailDownload { get; set; }

    public byte[] Content { get; set; } = [1, 2, 3];

    public StorageObjectInfo? Metadata { get; set; }

    public Exception? MetadataException { get; set; }

    public int DownloadCallCount { get; private set; }

    public int MetadataCallCount { get; private set; }

    public int PromoteCallCount { get; private set; }

    public int DeleteCallCount { get; private set; }

    public StoragePromotionRequest? LastPromotion { get; private set; }

    public Exception? PromotionException { get; set; }

    public List<StorageObjectLocation> DeletedLocations { get; } = [];

    public Task<StorageObjectInfo> UploadAsync(
        StorageUploadRequest request,
        CancellationToken cancellationToken)
    {
        Events.Add("upload");
        if (FailUpload)
        {
            throw new StorageOperationException(
                "Expected test failure.",
                new InvalidOperationException());
        }

        return Task.FromResult(
            new StorageObjectInfo(
                request.Location.Bucket,
                request.Location.ObjectKey,
                request.ContentType,
                request.Size,
                "etag",
                new string('a', 64)));
    }

    public async Task<StorageObjectInfo> DownloadAsync(
        StorageDownloadRequest request,
        CancellationToken cancellationToken)
    {
        Events.Add("download");
        DownloadCallCount++;
        if (FailDownload)
        {
            throw new StorageOperationException(
                "Expected download failure.",
                new InvalidOperationException());
        }

        await request.Destination.WriteAsync(Content, cancellationToken);
        return new StorageObjectInfo(
            request.Location.Bucket,
            request.Location.ObjectKey,
            "image/png",
            Content.LongLength,
            "etag");
    }

    public Task<StorageObjectInfo> GetMetadataAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken)
    {
        MetadataCallCount++;
        if (MetadataException is not null)
        {
            throw MetadataException;
        }
        return Metadata is null
            ? throw new StorageOperationException(
                "Expected missing object.",
                new InvalidOperationException())
            : Task.FromResult(Metadata);
    }

    public Task<StorageObjectInfo> PromoteAsync(
        StoragePromotionRequest request,
        CancellationToken cancellationToken)
    {
        PromoteCallCount++;
        LastPromotion = request;
        if (PromotionException is not null)
        {
            throw PromotionException;
        }
        var source = Metadata ?? throw new StorageObjectNotFoundException("Missing staging object.");
        return Task.FromResult(source with
        {
            Bucket = request.Destination.Bucket,
            ObjectKey = request.Destination.ObjectKey,
        });
    }

    public Task<bool> ExistsAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task DeleteAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken)
    {
        Events.Add("delete");
        DeleteCallCount++;
        DeletedLocations.Add(location);
        return Task.CompletedTask;
    }
}
