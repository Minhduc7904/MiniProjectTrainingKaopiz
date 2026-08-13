using MediaService.Application.Abstractions.Storage;

namespace MediaService.UnitTests.TestDoubles;

public sealed class StubStorage(List<string>? sharedEvents = null) : IStorage
{
    public List<string> Events { get; } = sharedEvents ?? [];

    public bool FailUpload { get; set; }

    public bool FailDownload { get; set; }

    public byte[] Content { get; set; } = [1, 2, 3];

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
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<bool> ExistsAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task DeleteAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken)
    {
        Events.Add("delete");
        return Task.CompletedTask;
    }
}
