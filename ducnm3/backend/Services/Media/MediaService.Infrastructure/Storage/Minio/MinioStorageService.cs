// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/MinioStorageService.cs
// Mục đích: Adapter MinIO triển khai lưu, stat, promote và xóa Media object; phân biệt object thiếu với sự cố storage.

using System.Buffers;
using MediaService.Application.Services.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace MediaService.Infrastructure.Storage.Minio;

public sealed partial class MinioStorageService(
    MinioInternalClient internalClient,
    IOptions<MinioStorageOptions> options,
    ILogger<MinioStorageService> logger) : IStorage, IStorageHealthProbe
{
    private readonly IMinioClient client = internalClient.Client;
    private readonly MinioStorageOptions storageOptions = options.Value;

    public async Task<StorageObjectInfo> UploadAsync(
        StorageUploadRequest request,
        CancellationToken cancellationToken)
    {
        var validated = MinioStorageRequestValidator.ValidateUpload(request);
        MinioStorageRequestValidator.ValidateLocation(
            request.Location,
            storageOptions.GetBuckets());

        try
        {
            using var hashingStream = new Sha256ReadStream(request.Content);
            var result = await client.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(request.Location.Bucket)
                    .WithObject(request.Location.ObjectKey)
                    .WithStreamData(hashingStream)
                    .WithObjectSize(request.Size)
                    .WithContentType(validated.ContentType),
                cancellationToken);

            return new StorageObjectInfo(
                request.Location.Bucket,
                request.Location.ObjectKey,
                validated.ContentType,
                result.Size,
                result.Etag,
                hashingStream.GetChecksumHex());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CreateOperationException("upload", exception);
        }
    }

    public async Task<StorageObjectInfo> DownloadAsync(
        StorageDownloadRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.Destination.CanWrite)
        {
            throw new StorageValidationException("The download destination stream must be writable.");
        }
        if (request.Offset < 0 || request.Length is <= 0)
        {
            throw new StorageValidationException("The requested download range is invalid.");
        }

        MinioStorageRequestValidator.ValidateLocation(request.Location, storageOptions.GetBuckets());

        try
        {
            var arguments = new GetObjectArgs()
                .WithBucket(request.Location.Bucket)
                .WithObject(request.Location.ObjectKey)
                .WithCallbackStream(
                    (source, token) => request.Length is null
                        ? source.CopyToAsync(request.Destination, token)
                        : CopyRangeAsync(
                            source,
                            request.Destination,
                            request.Offset,
                            request.Length.Value,
                            token));

            var result = await client.GetObjectAsync(
                arguments,
                cancellationToken);

            return MapObjectInfo(request.Location, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            StorageLog.DownloadFailed(logger, exception);
            throw CreateOperationException("download", exception);
        }
    }

    public async Task<StorageObjectInfo> GetMetadataAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken)
    {
        MinioStorageRequestValidator.ValidateLocation(location, storageOptions.GetBuckets());

        try
        {
            var result = await StatObjectAsync(location, cancellationToken);
            return MapObjectInfo(location, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ObjectNotFoundException exception)
        {
            throw new StorageObjectNotFoundException(exception.Message);
        }
        catch (Exception exception)
        {
            throw CreateOperationException("read metadata for", exception);
        }
    }

    public async Task<StorageObjectInfo> PromoteAsync(
        StoragePromotionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        MinioStorageRequestValidator.ValidateLocation(request.Source, storageOptions.GetBuckets());
        MinioStorageRequestValidator.ValidateLocation(request.Destination, storageOptions.GetBuckets());
        try
        {
            var conditions = new CopyConditions();
            conditions.SetMatchETag(request.SourceETag);
            await client.CopyObjectAsync(
                new CopyObjectArgs()
                    .WithBucket(request.Destination.Bucket)
                    .WithObject(request.Destination.ObjectKey)
                    .WithCopyObjectSource(
                        new CopySourceObjectArgs()
                            .WithBucket(request.Source.Bucket)
                            .WithObject(request.Source.ObjectKey)
                            .WithCopyConditions(conditions)),
                cancellationToken);
            return await GetMetadataAsync(request.Destination, cancellationToken);
        }
        catch (StorageObjectNotFoundException)
        {
            throw;
        }
        catch (ObjectNotFoundException exception)
        {
            throw new StorageObjectNotFoundException(exception.Message);
        }
        catch (PreconditionFailedException)
        {
            throw new StorageObjectNotFoundException(
                "The staging object changed before promotion.");
        }
        catch (MinioException exception) when (
            exception.Message.Contains("precondition", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("412", StringComparison.Ordinal))
        {
            throw new StorageObjectNotFoundException(
                "The staging object changed before promotion.");
        }
        catch (Exception exception)
        {
            throw CreateOperationException("promote", exception);
        }
    }

    public async Task<bool> ExistsAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken)
    {
        MinioStorageRequestValidator.ValidateLocation(location, storageOptions.GetBuckets());

        try
        {
            await StatObjectAsync(location, cancellationToken);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CreateOperationException("check existence of", exception);
        }
    }

    public async Task DeleteAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken)
    {
        MinioStorageRequestValidator.ValidateLocation(location, storageOptions.GetBuckets());

        try
        {
            await client.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(location.Bucket)
                    .WithObject(location.ObjectKey),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CreateOperationException("delete", exception);
        }
    }

    public async Task<StorageHealthProbeResult> CheckAsync(CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(storageOptions.HealthTimeoutSeconds));

        try
        {
            foreach (var bucket in storageOptions.GetBuckets())
            {
                var exists = await client.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucket),
                    timeoutSource.Token);
                if (!exists)
                {
                    StorageLog.DependencyUnavailable(logger, null);
                    return new StorageHealthProbeResult(false);
                }
            }

            return new StorageHealthProbeResult(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            StorageLog.DependencyUnavailable(logger, exception);
            return new StorageHealthProbeResult(false);
        }
    }

    private Task<ObjectStat> StatObjectAsync(
        StorageObjectLocation location,
        CancellationToken cancellationToken) =>
        client.StatObjectAsync(
            new StatObjectArgs()
                .WithBucket(location.Bucket)
                .WithObject(location.ObjectKey),
            cancellationToken);

    private static StorageObjectInfo MapObjectInfo(
        StorageObjectLocation location,
        ObjectStat result)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in result.MetaData)
        {
            var key = pair.Key.StartsWith("x-amz-meta-", StringComparison.OrdinalIgnoreCase)
                ? pair.Key[11..]
                : pair.Key;
            metadata[key.ToLowerInvariant()] = pair.Value;
        }

        return new(
            location.Bucket,
            location.ObjectKey,
            result.ContentType,
            result.Size,
            result.ETag,
            Metadata: metadata);
    }

    private static async Task CopyRangeAsync(
        Stream source,
        Stream destination,
        long offset,
        long length,
        CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(81_920);
        try
        {
            var remainingOffset = offset;
            while (remainingOffset > 0)
            {
                var read = await source.ReadAsync(
                    buffer.AsMemory(0, (int)Math.Min(buffer.Length, remainingOffset)),
                    cancellationToken);
                if (read == 0)
                {
                    throw new EndOfStreamException(
                        "The media object ended before the requested range.");
                }

                remainingOffset -= read;
            }

            var remainingLength = length;
            while (remainingLength > 0)
            {
                var read = await source.ReadAsync(
                    buffer.AsMemory(0, (int)Math.Min(buffer.Length, remainingLength)),
                    cancellationToken);
                if (read == 0)
                {
                    throw new EndOfStreamException(
                        "The media object ended before the requested range.");
                }

                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                remainingLength -= read;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static StorageOperationException CreateOperationException(
        string operation,
        Exception exception) =>
        new($"Unable to {operation} the media object in storage.", exception);

    private static partial class StorageLog
    {
        [LoggerMessage(
            EventId = 2201,
            Level = LogLevel.Warning,
            Message = "Media storage health probe failed.")]
        public static partial void DependencyUnavailable(ILogger logger, Exception? exception);

        [LoggerMessage(
            EventId = 2202,
            Level = LogLevel.Warning,
            Message = "Media storage download failed.")]
        public static partial void DownloadFailed(ILogger logger, Exception exception);
    }
}
