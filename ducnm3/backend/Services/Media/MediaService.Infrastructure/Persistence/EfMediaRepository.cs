using System.Data;
using MediaService.Application;
using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Abstractions.Storage;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Actors;
using MediaService.Domain.Media;
using MediaService.Domain.Usages;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace MediaService.Infrastructure.Persistence;

public sealed class EfMediaRepository(
    MediaDbContext dbContext,
    TimeProvider timeProvider) : IMediaRepository
{
    public async Task EnsureNotificationBodyUsagesAsync(
        Guid notificationId,
        Guid createdBy,
        IReadOnlyList<NotificationMediaUsageReferenceV1> references,
        CancellationToken cancellationToken)
    {
        var distinctReferences = references
            .GroupBy(reference => (reference.MediaId, reference.UsageType))
            .Select(group => group.OrderBy(reference => reference.DisplayOrder).First())
            .OrderBy(reference => reference.DisplayOrder)
            .ToArray();
        var mediaIds = distinctReferences
            .Select(reference => reference.MediaId)
            .Distinct()
            .ToList();

        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken)
            : null;
        var readyMediaIds = await dbContext.MediaObjects
            .AsNoTracking()
            .Where(media =>
                mediaIds.Contains(media.Id) &&
                media.Status == MediaObjectStatuses.Ready &&
                media.DeletedAt == null)
            .Select(media => media.Id)
            .ToListAsync(cancellationToken);
        if (readyMediaIds.Count != mediaIds.Count)
        {
            throw MediaErrors.MediaNotReady();
        }

        var existing = await dbContext.MediaUsages
            .AsNoTracking()
            .Where(usage =>
                usage.OwnerService == MediaOwnerServices.Notification &&
                usage.OwnerType == MediaOwnerTypes.NotificationBody &&
                usage.OwnerId == notificationId &&
                usage.DeletedAt == null &&
                mediaIds.Contains(usage.MediaId))
            .Select(usage => new { usage.MediaId, usage.UsageType })
            .ToListAsync(cancellationToken);
        var existingKeys = existing
            .Select(usage => (usage.MediaId, usage.UsageType))
            .ToHashSet();
        foreach (var reference in distinctReferences)
        {
            if (existingKeys.Contains((reference.MediaId, reference.UsageType)))
            {
                continue;
            }

            dbContext.MediaUsages.Add(new MediaUsage
            {
                Id = Guid.NewGuid(),
                MediaId = reference.MediaId,
                OwnerService = MediaOwnerServices.Notification,
                OwnerType = MediaOwnerTypes.NotificationBody,
                OwnerId = notificationId,
                UsageType = reference.UsageType,
                DisplayOrder = reference.DisplayOrder,
                CreatedBy = createdBy,
                CreatedByType = ActorTypes.Admin,
                CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            });
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is MySqlException { Number: 1062 })
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    public async Task AddPendingAsync(
        PendingMediaRecord media,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        dbContext.MediaObjects.Add(
            new MediaObject
            {
                Id = media.Id,
                Bucket = media.Location.Bucket,
                ObjectKey = media.Location.ObjectKey,
                MediaType = media.MediaType,
                ContentType = media.ContentType,
                OriginalFileName = media.OriginalFileName,
                SizeBytes = checked((ulong)media.SizeBytes),
                ChecksumSha256 = null,
                UploadedBy = media.UploadedBy.Id,
                UploadedByType = media.UploadedBy.Type,
                Status = MediaObjectStatuses.Pending,
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
        var media = await dbContext.MediaObjects.SingleAsync(
            item => item.Id == mediaId,
            cancellationToken);
        media.Status = MediaObjectStatuses.Ready;
        media.ChecksumSha256 = checksumSha256;
        media.CompletedAt = completedAtUtc;
        media.UpdatedAt = completedAtUtc;
        media.FailureReason = null;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid mediaId,
        string failureReason,
        CancellationToken cancellationToken)
    {
        var media = await dbContext.MediaObjects.SingleOrDefaultAsync(
            item => item.Id == mediaId,
            cancellationToken);
        if (media is null)
        {
            return;
        }

        media.Status = MediaObjectStatuses.Failed;
        media.FailureReason = failureReason.Length <= 500
            ? failureReason
            : failureReason[..500];
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
                new StorageObjectLocation(
                    item.Bucket,
                    item.ObjectKey),
                item.MediaType,
                item.ContentType,
                item.OriginalFileName,
                (long)item.SizeBytes,
                item.Status,
                item.CreatedAt,
                item.DeletedAt,
                item.SourceMediaId,
                item.DerivationType))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<MediaUsageUrlRecord?> GetActiveUsageUrlByIdAsync(
        Guid usageId,
        CancellationToken cancellationToken) =>
        await ActiveUsageUrls()
            .Where(item => item.Usage.Id == usageId)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(
        MediaUsageOwnerQuery query,
        CancellationToken cancellationToken) =>
        await ActiveUsageUrls()
            .Where(item =>
                item.Usage.OwnerService == query.OwnerService &&
                item.Usage.OwnerType == query.OwnerType &&
                item.Usage.UsageType == query.UsageType &&
                item.Usage.OwnerId == query.OwnerId)
            .OrderBy(item => item.Usage.DisplayOrder)
            .ThenBy(item => item.Usage.Id)
            .ToListAsync(cancellationToken);

    public async Task<MediaUsageRecord> ReplaceStudentAvatarAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken) =>
        await ReplaceExclusiveUsageAsync(usage, cancellationToken);

    public async Task<MediaUsageRecord> ReplaceMediaThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
    {
        var canManageOwner = await dbContext.MediaObjects
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.Id == usage.OwnerId &&
                    item.SourceMediaId == null &&
                    item.Status == MediaObjectStatuses.Ready &&
                    item.DeletedAt == null &&
                    item.UploadedBy == usage.CreatedBy.Id &&
                    item.UploadedByType == usage.CreatedBy.Type,
                cancellationToken);
        if (!canManageOwner)
        {
            throw MediaErrors.OwnerNotFound();
        }

        var validThumbnail = await dbContext.MediaObjects
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.Id == usage.MediaId &&
                    item.Status == MediaObjectStatuses.Ready &&
                    item.DeletedAt == null &&
                    item.DerivationType == MediaDerivationTypes.Thumbnail &&
                    item.ContentType == "image/webp" &&
                    item.SourceMedia != null &&
                    item.SourceMedia.UploadedBy == usage.CreatedBy.Id &&
                    item.SourceMedia.UploadedByType == usage.CreatedBy.Type,
                cancellationToken);
        if (!validThumbnail)
        {
            throw MediaErrors.InvalidMedia(
                "mediaId must be a READY WebP thumbnail owned by the actor.");
        }

        return await ReplaceExclusiveUsageAsync(usage, cancellationToken);
    }

    private async Task<MediaUsageRecord> ReplaceExclusiveUsageAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var activeUsages = await dbContext.MediaUsages
            .Where(item =>
                item.OwnerService == usage.OwnerService &&
                item.OwnerType == usage.OwnerType &&
                item.UsageType == usage.UsageType &&
                item.OwnerId == usage.OwnerId &&
                item.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (activeUsages.Any(item =>
                item.MediaId == usage.MediaId &&
                item.OwnerService == usage.OwnerService &&
                item.OwnerType == usage.OwnerType &&
                item.UsageType == usage.UsageType))
        {
            throw MediaErrors.MediaUsageConflict();
        }

        foreach (var activeUsage in activeUsages)
        {
            activeUsage.DeletedAt = now;
        }

        var entity = new MediaUsage
        {
            Id = usage.Id,
            MediaId = usage.MediaId,
            OwnerService = usage.OwnerService,
            OwnerType = usage.OwnerType,
            OwnerId = usage.OwnerId,
            UsageType = usage.UsageType,
            DisplayOrder = usage.DisplayOrder,
            CreatedBy = usage.CreatedBy.Id,
            CreatedByType = usage.CreatedBy.Type,
            CreatedAt = now,
        };
        dbContext.MediaUsages.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is MySqlException { Number: 1062 })
        {
            throw MediaErrors.MediaUsageConflict();
        }

        return new MediaUsageRecord(
            entity.Id,
            entity.MediaId,
            entity.OwnerService,
            entity.OwnerType,
            entity.OwnerId,
            entity.UsageType,
            entity.DisplayOrder,
            entity.CreatedAt);
    }

    private IQueryable<MediaUsageUrlRecord> ActiveUsageUrls() =>
        dbContext.MediaUsages
            .AsNoTracking()
            .Where(usage => usage.DeletedAt == null)
            .Join(
                dbContext.MediaObjects.AsNoTracking().Where(media =>
                    media.DeletedAt == null &&
                    media.Status == MediaObjectStatuses.Ready &&
                    media.MediaType == MediaTypes.Image),
                usage => usage.MediaId,
                media => media.Id,
                (usage, media) => new MediaUsageUrlRecord(
                    new MediaUsageRecord(
                        usage.Id,
                        usage.MediaId,
                        usage.OwnerService,
                        usage.OwnerType,
                        usage.OwnerId,
                        usage.UsageType,
                        usage.DisplayOrder,
                        usage.CreatedAt),
                    new MediaRecord(
                        media.Id,
                        new StorageObjectLocation(media.Bucket, media.ObjectKey),
                        media.MediaType,
                        media.ContentType,
                        media.OriginalFileName,
                        (long)media.SizeBytes,
                        media.Status,
                        media.CreatedAt,
                        media.DeletedAt,
                        media.SourceMediaId,
                        media.DerivationType)));
}
