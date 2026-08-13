using MediaService.Application.Abstractions.Storage;

namespace MediaService.Application.Features.Media.GetContent;

public sealed class GetMediaContentResult
{
    private readonly IStorage storage;
    private readonly StorageObjectLocation location;

    internal GetMediaContentResult(
        Guid id,
        string mediaType,
        string contentType,
        string originalFileName,
        long sizeBytes,
        DateTime createdAtUtc,
        IStorage storage,
        StorageObjectLocation location)
    {
        Id = id;
        MediaType = mediaType;
        ContentType = contentType;
        OriginalFileName = originalFileName;
        SizeBytes = sizeBytes;
        CreatedAtUtc = createdAtUtc;
        this.storage = storage;
        this.location = location;
    }

    public Guid Id { get; }

    public string MediaType { get; }

    public string ContentType { get; }

    public string OriginalFileName { get; }

    public long SizeBytes { get; }

    public DateTime CreatedAtUtc { get; }

    public async Task CopyToAsync(
        Stream destination,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
        {
            throw new ArgumentException(
                "The destination stream must be writable.",
                nameof(destination));
        }

        try
        {
            await storage.DownloadAsync(
                new StorageDownloadRequest(location, destination),
                cancellationToken);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (StorageOperationException)
        {
            throw MediaErrors.StorageUnavailable();
        }
    }
}
