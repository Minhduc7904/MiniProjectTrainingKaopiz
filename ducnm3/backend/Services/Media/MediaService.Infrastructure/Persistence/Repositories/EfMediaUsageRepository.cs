// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Repositories/EfMediaUsageRepository.cs
// Mục đích: Triển khai repository EfMediaUsageRepository bằng EF Core và persistence model.

using System.Data;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using DatabaseMediaObject = MediaService.Infrastructure.Persistence.Scaffolded.MediaObject;
using DatabaseMediaUsage = MediaService.Infrastructure.Persistence.Scaffolded.MediaUsage;
using DomainMediaUsage = MediaService.Domain.Entities.MediaUsage;

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

        var attachedMedia = await dbContext.MediaObjects
            .Where(media => mediaIds.Contains(media.Id) && media.DeletedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var media in attachedMedia)
        {
            media.IsDraft = false;
            media.DraftedAt = null;
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
        return pair is null ? null : MapUsageUrl(pair.Usage, pair.Media, pair.Thumbnail);
    }

    public async Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(
        MediaUsageOwnerQuery query,
        CancellationToken cancellationToken)
    {
        var ownerIds = query.OwnerIds.ToArray();
        var pairs = await ActiveImageUsages(
                query.OwnerService,
                query.OwnerType,
                query.UsageType,
                ownerIds)
            .ToListAsync(cancellationToken);
        return pairs.Select(pair => MapUsageUrl(pair.Usage, pair.Media, pair.Thumbnail)).ToArray();
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

    public async Task<DomainMediaUsage> ReplaceCourseThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
    {
        await EnsureCourseMediaAsync(usage, requireThumbnail: true, cancellationToken);
        return await ReplaceExclusiveUsageAsync(usage, cancellationToken);
    }

    public async Task<DomainMediaUsage> AddCourseGalleryMediaAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
    {
        await EnsureCourseMediaAsync(usage, requireThumbnail: false, cancellationToken);
        await EnsureMediaUsagesAsync([usage], cancellationToken);
        return new DomainMediaUsage(
            usage.Id,
            usage.MediaId,
            usage.OwnerService,
            usage.OwnerType,
            usage.OwnerId,
            usage.UsageType,
            usage.DisplayOrder,
            usage.CreatedBy,
            timeProvider.GetUtcNow().UtcDateTime);
    }

    public async Task EnsureCourseLessonMediaAsync(
        IReadOnlyList<CreateMediaUsageRecord> usages,
        CancellationToken cancellationToken)
    {
        var mediaIds = usages.Select(item => item.MediaId).Distinct().ToList();
        var readyCount = await dbContext.MediaObjects.CountAsync(
            item => mediaIds.Contains(item.Id) && item.Status == MediaObjectStatuses.Ready &&
                item.DeletedAt == null && item.SourceMediaId == null,
            cancellationToken);
        if (readyCount != mediaIds.Count)
        {
            throw MediaErrors.MediaNotReady();
        }

        await EnsureMediaUsagesAsync(usages, cancellationToken);
    }

    public async Task RemoveAsync(Guid usageId, ActorReference actor, CancellationToken cancellationToken)
    {
        var usage = await dbContext.MediaUsages
            .SingleOrDefaultAsync(item => item.Id == usageId && item.DeletedAt == null, cancellationToken)
            ?? throw MediaErrors.MediaUsageNotFound();
        if (usage.OwnerService == MediaOwnerServices.Course && actor.Type != ActorTypes.Admin)
        {
            throw MediaErrors.InvalidActorType("Only ADMIN actors can manage Course media.");
        }

        usage.DeletedAt = timeProvider.GetUtcNow().UtcDateTime;
        var hasActiveUsage = await dbContext.MediaUsages.AnyAsync(
            item => item.MediaId == usage.MediaId && item.DeletedAt == null && item.Id != usage.Id,
            cancellationToken);
        if (!hasActiveUsage)
        {
            var media = await dbContext.MediaObjects.SingleAsync(item => item.Id == usage.MediaId, cancellationToken);
            media.IsDraft = true;
            media.DraftedAt = timeProvider.GetUtcNow().UtcDateTime;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveCourseContentMediaAsync(
        IReadOnlyList<CourseContentMediaUsageRemoval> removals,
        CancellationToken cancellationToken)
    {
        if (removals.Count == 0)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var removal in removals.Distinct())
        {
            var usages = await dbContext.MediaUsages
                .Where(item => item.OwnerService == MediaOwnerServices.Course &&
                    item.OwnerType == removal.OwnerType &&
                    item.OwnerId == removal.OwnerId &&
                    item.MediaId == removal.MediaId &&
                    item.UsageType == removal.UsageType &&
                    item.DeletedAt == null)
                .ToListAsync(cancellationToken);
            if (usages.Count == 0)
            {
                continue;
            }

            foreach (var usage in usages)
            {
                usage.DeletedAt = now;
            }

            var hasOtherActiveUsage = await dbContext.MediaUsages.AnyAsync(
                item => item.MediaId == removal.MediaId && item.DeletedAt == null &&
                    !usages.Select(usage => usage.Id).Contains(item.Id),
                cancellationToken);
            if (!hasOtherActiveUsage)
            {
                var media = await dbContext.MediaObjects.SingleAsync(
                    item => item.Id == removal.MediaId,
                    cancellationToken);
                media.IsDraft = true;
                media.DraftedAt = now;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderAsync(
        string ownerService,
        string ownerType,
        Guid ownerId,
        IReadOnlyList<Guid> usageIds,
        ActorReference actor,
        CancellationToken cancellationToken)
    {
        if (ownerService == MediaOwnerServices.Course && actor.Type != ActorTypes.Admin)
        {
            throw MediaErrors.InvalidActorType("Only ADMIN actors can manage Course media.");
        }

        var usages = await dbContext.MediaUsages
            .Where(item => item.OwnerService == ownerService && item.OwnerType == ownerType &&
                item.OwnerId == ownerId && item.DeletedAt == null)
            .ToListAsync(cancellationToken);
        if (usages.Count != usageIds.Count || usages.Any(item => !usageIds.Contains(item.Id)))
        {
            throw MediaErrors.InvalidMedia("usageIds must contain exactly the active usages for the owner.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        for (var index = 0; index < usageIds.Count; index++)
        {
            usages.Single(item => item.Id == usageIds[index]).DisplayOrder = (uint)index;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task EnsureCourseMediaAsync(
        CreateMediaUsageRecord usage,
        bool requireThumbnail,
        CancellationToken cancellationToken)
    {
        var media = await dbContext.MediaObjects
            .AsNoTracking()
            .Where(item => item.Id == usage.MediaId &&
                item.Status == MediaObjectStatuses.Ready &&
                item.DeletedAt == null &&
                item.UploadedBy == usage.CreatedBy.Id &&
                item.UploadedByType == usage.CreatedBy.Type)
            .Select(item => new { item.MediaType, item.SourceMediaId, item.DerivationType, item.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        if (media is null)
        {
            throw MediaErrors.MediaNotReady();
        }

        var valid = requireThumbnail
            ? media.SourceMediaId is not null &&
              media.DerivationType == MediaDerivationTypes.Thumbnail &&
              string.Equals(media.ContentType, "image/webp", StringComparison.OrdinalIgnoreCase)
            : media.MediaType == MediaTypes.Image && media.SourceMediaId is null;
        if (!valid)
        {
            throw MediaErrors.InvalidMedia(requireThumbnail
                ? "Course thumbnail must be a READY WebP thumbnail owned by the actor."
                : "Course gallery media must be a READY original image owned by the actor.");
        }
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

    private IQueryable<ActiveImageUsage> ActiveImageUsages(
        string? ownerService = null,
        string? ownerType = null,
        string? usageType = null,
        IReadOnlyList<Guid>? ownerIds = null) =>
        dbContext.MediaUsages.AsNoTracking()
            .Include(usage => usage.Media)
                .ThenInclude(media => media.SourceMedia)
            .Include(usage => usage.Media)
                .ThenInclude(media => media.InverseSourceMedia)
            .Where(usage => usage.DeletedAt == null &&
                usage.Media.DeletedAt == null &&
                usage.Media.Status == MediaObjectStatuses.Ready &&
                (ownerService == null || usage.OwnerService == ownerService) &&
                (ownerType == null || usage.OwnerType == ownerType) &&
                (usageType == null || usage.UsageType == usageType) &&
                (ownerIds == null || Enumerable.Contains(ownerIds, usage.OwnerId)))
            .OrderBy(usage => usage.DisplayOrder)
            .ThenBy(usage => usage.Id)
            .Select(usage => new ActiveImageUsage(
                usage,
                usage.Media.SourceMedia ?? usage.Media,
                usage.Media.SourceMedia == null
                    ? usage.Media.InverseSourceMedia
                        .Where(media => media.DeletedAt == null &&
                            media.Status == MediaObjectStatuses.Ready &&
                            media.DerivationType == MediaDerivationTypes.Thumbnail)
                        .OrderByDescending(media => media.CompletedAt)
                        .FirstOrDefault()
                    : usage.Media));

    private static MediaUsageUrlRecord MapUsageUrl(
        DatabaseMediaUsage usage,
        DatabaseMediaObject media,
        DatabaseMediaObject? thumbnail)
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
            ToMediaRecord(media),
            thumbnail is null || thumbnail.Id == media.Id ? null : ToMediaRecord(thumbnail));
    }

    private static MediaRecord ToMediaRecord(DatabaseMediaObject media)
    {
        var domainMedia = MediaPersistenceMapper.ToDomain(media);
        return new MediaRecord(
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
            domainMedia.UploadedBy);
    }

    private sealed record ActiveImageUsage(
        DatabaseMediaUsage Usage,
        DatabaseMediaObject Media,
        DatabaseMediaObject? Thumbnail);
}
