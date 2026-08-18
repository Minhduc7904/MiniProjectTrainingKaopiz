// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Repositories/EfMediaDerivationRepository.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Application;
using MediaService.Application.Repositories;
using MediaService.Application.Contracts.Messaging;
using MediaService.Application.Services.Storage;
using MediaService.Domain.ValueObjects;
using MediaService.Domain.Constants;
using MediaService.Infrastructure.Persistence.Scaffolded;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

using MediaService.Application.Common.Errors;

namespace MediaService.Infrastructure.Persistence.Repositories;

public sealed class EfMediaDerivationRepository(
    MediaDbContext dbContext,
    ICommandSender commandSender,
    IMediaUsageRepository mediaUsageRepository,
    TimeProvider timeProvider) : IMediaDerivationRepository
{
    public async Task<ThumbnailDerivationWork?> BeginAsync(
        Guid jobId,
        Guid sourceMediaId,
        Guid derivativeMediaId,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.MediaDerivationJobs
            .Include(item => item.SourceMedia)
            .Include(item => item.DerivativeMedia)
            .SingleOrDefaultAsync(
                item =>
                    item.Id == jobId &&
                    item.SourceMediaId == sourceMediaId &&
                    item.DerivativeMediaId == derivativeMediaId,
                cancellationToken);
        if (job is null ||
            job.SourceMedia.DeletedAt is not null ||
            job.SourceMedia.Status != MediaObjectStatuses.Ready)
        {
            return null;
        }

        if (job.Status != MediaDerivationStatuses.Ready)
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            job.Status = MediaDerivationStatuses.Processing;
            job.AttemptCount++;
            job.StartedAt = now;
            job.CompletedAt = null;
            job.LastError = null;
            job.UpdatedAt = now;
            job.DerivativeMedia.Status = MediaObjectStatuses.Pending;
            job.DerivativeMedia.FailureReason = null;
            job.DerivativeMedia.UpdatedAt = now;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return MediaDerivationPersistenceMapper.ToThumbnailDerivationWork(job);
    }

    public async Task CompleteAsync(
        Guid jobId,
        Guid derivativeMediaId,
        string checksumSha256,
        long sizeBytes,
        DateTime completedAtUtc,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.MediaDerivationJobs
            .Include(item => item.SourceMedia)
            .Include(item => item.DerivativeMedia)
            .SingleAsync(
                item => item.Id == jobId &&
                    item.DerivativeMediaId == derivativeMediaId,
                cancellationToken);
        if (job.Status == MediaDerivationStatuses.Ready &&
            job.DerivativeMedia.Status == MediaObjectStatuses.Ready)
        {
            return;
        }

        job.DerivativeMedia.Status = MediaObjectStatuses.Ready;
        job.DerivativeMedia.ChecksumSha256 = checksumSha256;
        job.DerivativeMedia.SizeBytes = checked((ulong)sizeBytes);
        job.DerivativeMedia.CompletedAt = completedAtUtc;
        job.DerivativeMedia.UpdatedAt = completedAtUtc;
        job.DerivativeMedia.FailureReason = null;
        job.Status = MediaDerivationStatuses.Ready;
        job.CompletedAt = completedAtUtc;
        job.UpdatedAt = completedAtUtc;
        job.LastError = null;

        await mediaUsageRepository.EnsureMediaUsagesAsync(
            [
                new CreateMediaUsageRecord(
                    Guid.NewGuid(),
                    derivativeMediaId,
                    MediaOwnerServices.Media,
                    MediaOwnerTypes.MediaThumbnail,
                    job.SourceMediaId,
                    MediaUsageTypes.Thumbnail,
                    0,
                    new ActorReference(
                        job.SourceMedia.UploadedByType,
                        job.SourceMedia.UploadedBy)),
            ],
            cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid jobId,
        Guid derivativeMediaId,
        string safeError,
        DateTime failedAtUtc,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.MediaDerivationJobs
            .Include(item => item.DerivativeMedia)
            .SingleOrDefaultAsync(
                item => item.Id == jobId &&
                    item.DerivativeMediaId == derivativeMediaId,
                cancellationToken);
        if (job is null || job.Status == MediaDerivationStatuses.Ready)
        {
            return;
        }

        job.Status = MediaDerivationStatuses.Failed;
        job.LastError = Truncate(safeError);
        job.CompletedAt = failedAtUtc;
        job.UpdatedAt = failedAtUtc;
        job.DerivativeMedia.Status = MediaObjectStatuses.Failed;
        job.DerivativeMedia.FailureReason = Truncate(safeError);
        job.DerivativeMedia.UpdatedAt = failedAtUtc;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MediaThumbnailStatusRecord?> GetThumbnailStatusAsync(
        Guid sourceMediaId,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.MediaDerivationJobs
            .AsNoTracking()
            .Where(item =>
                item.SourceMediaId == sourceMediaId &&
                item.SourceMedia.DeletedAt == null)
            .Select(item => new
            {
                item.Id,
                item.Status,
                item.DerivativeMediaId,
                item.LastError,
                item.UpdatedAt,
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (job is null)
        {
            return null;
        }

        return new MediaThumbnailStatusRecord(
            sourceMediaId,
            job.Id,
            job.Status,
            job.DerivativeMediaId,
            await dbContext.MediaUsages
                .AsNoTracking()
                .Where(usage =>
                    usage.OwnerService == MediaOwnerServices.Media &&
                    usage.OwnerType == MediaOwnerTypes.MediaThumbnail &&
                    usage.OwnerId == sourceMediaId &&
                    usage.UsageType == MediaUsageTypes.Thumbnail &&
                    usage.DeletedAt == null)
                .Select(usage => (Guid?)usage.MediaId)
                .SingleOrDefaultAsync(cancellationToken),
            job.LastError,
            job.UpdatedAt);
    }

    public async Task<MediaThumbnailStatusRecord> RetryAsync(
        Guid sourceMediaId,
        ActorReference requestedBy,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken);
        var job = await dbContext.MediaDerivationJobs
            .Include(item => item.SourceMedia)
            .Include(item => item.DerivativeMedia)
            .SingleOrDefaultAsync(
                item => item.SourceMediaId == sourceMediaId,
                cancellationToken) ?? throw MediaErrors.ThumbnailNotFound();
        if (job.SourceMedia.UploadedBy != requestedBy.Id ||
            !string.Equals(
                job.SourceMedia.UploadedByType,
                requestedBy.Type,
                StringComparison.Ordinal))
        {
            throw MediaErrors.ThumbnailNotFound();
        }

        if (job.Status != MediaDerivationStatuses.Failed)
        {
            throw MediaErrors.ThumbnailRetryConflict();
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        job.Status = MediaDerivationStatuses.Queued;
        job.LastError = null;
        job.CompletedAt = null;
        job.UpdatedAt = now;
        job.DerivativeMedia.Status = MediaObjectStatuses.Pending;
        job.DerivativeMedia.FailureReason = null;
        job.DerivativeMedia.UpdatedAt = now;

        await commandSender.SendAsync(
            ServiceNames.Media,
            new GenerateMediaThumbnailV1(
                job.Id,
                job.SourceMediaId,
                job.DerivativeMediaId),
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new MediaThumbnailStatusRecord(
            sourceMediaId,
            job.Id,
            job.Status,
            job.DerivativeMediaId,
            null,
            null,
            now);
    }

    private static string Truncate(string value) =>
        value.Length <= 500 ? value : value[..500];
}
