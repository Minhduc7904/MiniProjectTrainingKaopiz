// File: backend/Services/Media/MediaService.Application/Repositories/IMediaDerivationRepository.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Services.Storage;
using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.Repositories;

public interface IMediaDerivationRepository
{
    Task<ThumbnailDerivationWork?> BeginAsync(
        Guid jobId,
        Guid sourceMediaId,
        Guid derivativeMediaId,
        CancellationToken cancellationToken);

    Task CompleteAsync(
        Guid jobId,
        Guid derivativeMediaId,
        string checksumSha256,
        long sizeBytes,
        DateTime completedAtUtc,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid jobId,
        Guid derivativeMediaId,
        string safeError,
        DateTime failedAtUtc,
        CancellationToken cancellationToken);

    Task<MediaThumbnailStatusRecord?> GetThumbnailStatusAsync(
        Guid sourceMediaId,
        CancellationToken cancellationToken);

    Task<MediaThumbnailStatusRecord> RetryAsync(
        Guid sourceMediaId,
        ActorReference requestedBy,
        CancellationToken cancellationToken);
}

public sealed record ThumbnailDerivationWork(
    Guid JobId,
    Guid SourceMediaId,
    StorageObjectLocation SourceLocation,
    string SourceMediaType,
    string SourceContentType,
    string SourceStatus,
    Guid DerivativeMediaId,
    StorageObjectLocation DerivativeLocation,
    string DerivativeStatus,
    string JobStatus,
    ActorReference UploadedBy);

public sealed record MediaThumbnailStatusRecord(
    Guid SourceMediaId,
    Guid JobId,
    string Status,
    Guid ThumbnailMediaId,
    Guid? ActiveThumbnailMediaId,
    string? LastError,
    DateTime UpdatedAtUtc);
