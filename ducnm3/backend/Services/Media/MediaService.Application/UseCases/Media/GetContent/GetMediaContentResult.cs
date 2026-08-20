// File: backend/Services/Media/MediaService.Application/UseCases/Media/GetContent/GetMediaContentResult.cs
// Mục đích: Định nghĩa dữ liệu đầu ra của use case GetMediaContentResult.

using MediaService.Application.Common.Errors;
using MediaService.Application.Services.Storage;

namespace MediaService.Application.UseCases.Media.GetContent;

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
        => await CopyToAsync(destination, 0, null, cancellationToken);

    public async Task CopyToAsync(
        Stream destination,
        long offset,
        long? length,
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
            if (offset < 0 || length is <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(offset),
                    "The requested byte range is invalid.");
            }

            await storage.DownloadAsync(
                new StorageDownloadRequest(location, destination, offset, length),
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
