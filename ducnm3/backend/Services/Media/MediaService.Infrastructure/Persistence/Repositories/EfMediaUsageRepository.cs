// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Repositories/EfMediaUsageRepository.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using System.Data;
using MediaService.Application;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Domain.Constants;
using MediaService.Infrastructure.Persistence.Mappers;
using MediaService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using DatabaseMediaObject = MediaService.Infrastructure.Persistence.Scaffolded.MediaObject;
using DatabaseMediaUsage = MediaService.Infrastructure.Persistence.Scaffolded.MediaUsage;
using DomainMediaUsage = MediaService.Domain.Entities.MediaUsage;

using MediaService.Application.Common.Errors;

namespace MediaService.Infrastructure.Persistence.Repositories;

public sealed class EfMediaUsageRepository(
    MediaDbContext dbContext,
    TimeProvider timeProvider) : IMediaUsageRepository
{
    public async Task EnsureMediaUsagesAsync(
        IReadOnlyList<CreateMediaUsageRecord> usages,
        CancellationToken cancellationToken)
    {
        var distinctUsages = usages
            .GroupBy(usage => (
                usage.MediaId,
                usage.OwnerService,
                usage.OwnerType,
                usage.OwnerId,
                usage.UsageType))
            .Select(group => group.OrderBy(usage => usage.DisplayOrder).First())
            .OrderBy(usage => usage.OwnerId)
            .ThenBy(usage => usage.DisplayOrder)
            .ToArray();
        var mediaIds = distinctUsages.Select(usage => usage.MediaId).Distinct().ToList();
        var ownerIds = distinctUsages.Select(usage => usage.OwnerId).Distinct().ToList();
        var ownerServices = distinctUsages.Select(usage => usage.OwnerService).Distinct().ToList();
        var ownerTypes = distinctUsages.Select(usage => usage.OwnerType).Distinct().ToList();
        var usageTypes = distinctUsages.Select(usage => usage.UsageType).Distinct().ToList();

        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;
        var readyMediaIds = await dbContext.MediaObjects
            .AsNoTracking()
            .Where(media => mediaIds.Contains(media.Id) &&
                media.Status == MediaObjectStatuses.Ready && media.DeletedAt == null)
            .Select(media => media.Id)
            .ToListAsync(cancellationToken);
        readyMediaIds.AddRange(dbContext.MediaObjects.Local
            .Where(media => mediaIds.Contains(media.Id) &&
                media.Status == MediaObjectStatuses.Ready && media.DeletedAt == null)
            .Select(media => media.Id));
        if (readyMediaIds.Distinct().Count() != mediaIds.Count)
        {
            throw MediaErrors.MediaNotReady();
        }

        var existingKeys = (await dbContext.MediaUsages
            .AsNoTracking()
            .Where(usage => usage.DeletedAt == null &&
                mediaIds.Contains(usage.MediaId) && ownerIds.Contains(usage.OwnerId) &&
                ownerServices.Contains(usage.OwnerService) && ownerTypes.Contains(usage.OwnerType) &&
                usageTypes.Contains(usage.UsageType))
            .Select(usage => new
            {
                usage.MediaId,
                usage.OwnerService,
                usage.OwnerType,
                usage.OwnerId,
                usage.UsageType,
            })
            .ToListAsync(cancellationToken))
            .Select(usage => (
                usage.MediaId, usage.OwnerService, usage.OwnerType, usage.OwnerId, usage.UsageType))
            .ToHashSet();

        foreach (var usage in distinctUsages.Where(usage => !existingKeys.Contains((
                     usage.MediaId, usage.OwnerService, usage.OwnerType, usage.OwnerId, usage.UsageType))))
        {
            dbContext.MediaUsages.Add(new DatabaseMediaUsage
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
        catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    public async Task<MediaUsageUrlRecord?> GetActiveUsageUrlByIdAsync(
        Guid usageId,
        CancellationToken cancellationToken)
    {
        var pair = await ActiveImageUsages()
            .SingleOrDefaultAsync(item => item.Usage.Id == usageId, cancellationToken);
        return pair is null ? null : MapUsageUrl(pair.Usage, pair.Media);
    }

    public async Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(
        MediaUsageOwnerQuery query,
        CancellationToken cancellationToken)
    {
        var pairs = await ActiveImageUsages()
            .Where(item => item.Usage.OwnerService == query.OwnerService &&
                item.Usage.OwnerType == query.OwnerType && item.Usage.UsageType == query.UsageType &&
                item.Usage.OwnerId == query.OwnerId)
            .OrderBy(item => item.Usage.DisplayOrder)
            .ThenBy(item => item.Usage.Id)
            .ToListAsync(cancellationToken);
        return pairs.Select(pair => MapUsageUrl(pair.Usage, pair.Media)).ToArray();
    }

    public Task<DomainMediaUsage> ReplaceStudentAvatarAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken) => ReplaceExclusiveUsageAsync(usage, cancellationToken);

    public async Task<DomainMediaUsage> ReplaceMediaThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
    {
        var canManageOwner = await dbContext.MediaObjects.AsNoTracking().AnyAsync(item =>
            item.Id == usage.OwnerId && item.SourceMediaId == null &&
            item.Status == MediaObjectStatuses.Ready && item.DeletedAt == null &&
            item.UploadedBy == usage.CreatedBy.Id && item.UploadedByType == usage.CreatedBy.Type,
            cancellationToken);
        if (!canManageOwner)
        {
            throw MediaErrors.OwnerNotFound();
        }

        var validThumbnail = await dbContext.MediaObjects.AsNoTracking().AnyAsync(item =>
            item.Id == usage.MediaId && item.Status == MediaObjectStatuses.Ready &&
            item.DeletedAt == null && item.DerivationType == MediaDerivationTypes.Thumbnail &&
            item.ContentType == "image/webp" && item.SourceMedia != null &&
            item.SourceMedia.UploadedBy == usage.CreatedBy.Id &&
            item.SourceMedia.UploadedByType == usage.CreatedBy.Type,
            cancellationToken);
        if (!validThumbnail)
        {
            throw MediaErrors.InvalidMedia("mediaId must be a READY WebP thumbnail owned by the actor.");
        }

        return await ReplaceExclusiveUsageAsync(usage, cancellationToken);
    }

    private async Task<DomainMediaUsage> ReplaceExclusiveUsageAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var activeUsages = await dbContext.MediaUsages.Where(item =>
            item.OwnerService == usage.OwnerService && item.OwnerType == usage.OwnerType &&
            item.UsageType == usage.UsageType && item.OwnerId == usage.OwnerId &&
            item.DeletedAt == null).ToListAsync(cancellationToken);
        if (activeUsages.Any(item => item.MediaId == usage.MediaId &&
            item.OwnerService == usage.OwnerService && item.OwnerType == usage.OwnerType &&
            item.UsageType == usage.UsageType))
        {
            throw MediaErrors.MediaUsageConflict();
        }

        foreach (var activeUsage in activeUsages)
        {
            activeUsage.DeletedAt = now;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await EnsureMediaUsagesAsync([usage], cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
        {
            throw MediaErrors.MediaUsageConflict();
        }

        return new DomainMediaUsage(
            usage.Id, usage.MediaId, usage.OwnerService, usage.OwnerType, usage.OwnerId,
            usage.UsageType, usage.DisplayOrder, usage.CreatedBy, now);
    }

    private IQueryable<ActiveImageUsage> ActiveImageUsages() =>
        dbContext.MediaUsages.AsNoTracking()
            .Where(usage => usage.DeletedAt == null)
            .Join(
                dbContext.MediaObjects.AsNoTracking().Where(media =>
                    media.DeletedAt == null && media.Status == MediaObjectStatuses.Ready &&
                    media.MediaType == MediaTypes.Image),
                usage => usage.MediaId,
                media => media.Id,
                (usage, media) => new ActiveImageUsage(usage, media));

    private static MediaUsageUrlRecord MapUsageUrl(DatabaseMediaUsage usage, DatabaseMediaObject media)
    {
        var domainUsage = MediaUsagePersistenceMapper.ToDomain(usage);
        var domainMedia = MediaPersistenceMapper.ToDomain(media);
        return new MediaUsageUrlRecord(
            new MediaUsageRecord(
                domainUsage.Id,
                domainUsage.MediaId,
                domainUsage.OwnerService,
                domainUsage.OwnerType,
                domainUsage.OwnerId,
                domainUsage.UsageType,
                domainUsage.DisplayOrder,
                domainUsage.CreatedAtUtc),
            new MediaRecord(
                domainMedia.Id,
                new StorageObjectLocation(domainMedia.Bucket, domainMedia.ObjectKey),
                domainMedia.MediaType,
                domainMedia.ContentType,
                domainMedia.OriginalFileName,
                domainMedia.SizeBytes,
                domainMedia.Status,
                domainMedia.CreatedAtUtc,
                domainMedia.DeletedAtUtc,
                domainMedia.SourceMediaId,
                domainMedia.DerivationType,
                domainMedia.IsDraft,
                domainMedia.DraftedAtUtc,
                domainMedia.ChecksumSha256,
                domainMedia.UploadedBy));
    }

    private sealed record ActiveImageUsage(DatabaseMediaUsage Usage, DatabaseMediaObject Media);
}
