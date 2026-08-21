// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Repositories/EfMediaRepository.cs
// Mục đích: Triển khai repository EfMediaRepository bằng EF Core và persistence model.

using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence.Repositories;

public sealed class EfMediaRepository(
    MediaDbContext dbContext,
    EfMediaBackgroundJobRepository backgroundJobs,
    TimeProvider timeProvider) : IMediaRepository
{
    public async Task AddPendingAsync(
        PendingMediaRecord media,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        dbContext.MediaObjects.Add(new MediaObject
        {
            Id = media.Id,
            Bucket = media.Location.Bucket,
            ObjectKey = media.Location.ObjectKey,
            MediaType = media.MediaType,
            ContentType = media.ContentType,
            OriginalFileName = media.OriginalFileName,
            SizeBytes = checked((ulong)media.SizeBytes),
            ChecksumSha256 = media.ExpectedChecksumSha256,
            UploadedBy = media.UploadedBy.Id,
            UploadedByType = media.UploadedBy.Type,
            Status = MediaObjectStatuses.Pending,
            IsDraft = media.IsDraft,
            DraftedAt = media.DraftedAtUtc,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkReadyAsync(
        Guid mediaId,
        string checksumSha256,
        DateTime completedAtUtc,
        CancellationToken cancellationToken)
    {
        var media = await dbContext.MediaObjects.SingleAsync(item => item.Id == mediaId, cancellationToken);
        media.Status = MediaObjectStatuses.Ready;
        media.ChecksumSha256 = checksumSha256;
        media.CompletedAt = completedAtUtc;
        media.IsDraft = true;
        media.DraftedAt = completedAtUtc;
        media.UpdatedAt = completedAtUtc;
        media.FailureReason = null;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid mediaId,
        string failureReason,
        CancellationToken cancellationToken)
    {
        var media = await dbContext.MediaObjects.SingleOrDefaultAsync(item => item.Id == mediaId, cancellationToken);
        if (media is null)
        {
            return;
        }

        media.Status = MediaObjectStatuses.Failed;
        media.FailureReason = failureReason.Length <= 500 ? failureReason : failureReason[..500];
        media.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MediaRecord?> GetByIdAsync(
        Guid mediaId,
        CancellationToken cancellationToken) =>
        await dbContext.MediaObjects
            .AsNoTracking()
            .Where(item => item.Id == mediaId)
            .Select(item => new MediaRecord(
                item.Id,
                new StorageObjectLocation(item.Bucket, item.ObjectKey),
                item.MediaType,
                item.ContentType,
                item.OriginalFileName,
                (long)item.SizeBytes,
                item.Status,
                item.CreatedAt,
                item.DeletedAt,
                item.SourceMediaId,
                item.DerivationType,
                item.IsDraft,
                item.DraftedAt,
                item.ChecksumSha256,
                new ActorReference(item.UploadedByType, item.UploadedBy)))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<MediaLibraryRecord>> ListByActorAsync(
        ActorReference actor,
        string? mediaType,
        (DateTime CreatedAtUtc, Guid Id)? cursor,
        int take,
        CancellationToken cancellationToken)
    {
        var query = dbContext.MediaObjects
            .AsNoTracking()
            .Where(item => item.UploadedBy == actor.Id &&
                item.UploadedByType == actor.Type &&
                item.SourceMediaId == null &&
                item.DeletedAt == null);
        if (mediaType is not null)
            query = query.Where(item => item.MediaType == mediaType);
        if (cursor is { } value)
        {
            query = query.Where(item => item.CreatedAt < value.CreatedAtUtc ||
                (item.CreatedAt == value.CreatedAtUtc && item.Id.CompareTo(value.Id) < 0));
        }

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .Take(take)
            .Select(item => new MediaLibraryRecord(
                item.Id,
                item.MediaType,
                item.ContentType,
                item.OriginalFileName,
                (long)item.SizeBytes,
                item.Status,
                item.IsDraft,
                item.DraftedAt,
                item.CreatedAt,
                item.CompletedAt,
                backgroundJobs.Query()
                    .Where(job => job.JobType == MediaBackgroundJobTypes.ThumbnailDerivation && job.SubjectId == item.Id)
                    .Select(job => (Guid?)null)
                    .FirstOrDefault(),
                backgroundJobs.Query()
                    .Where(job => job.JobType == MediaBackgroundJobTypes.ThumbnailDerivation && job.SubjectId == item.Id)
                    .Select(job => job.Status)
                    .FirstOrDefault()))
            .ToArrayAsync(cancellationToken);
    }
}
