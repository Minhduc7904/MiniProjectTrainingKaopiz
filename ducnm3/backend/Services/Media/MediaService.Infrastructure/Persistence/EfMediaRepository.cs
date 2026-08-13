using System.Data;
using MediaService.Application;
using MediaService.Application.Persistence;
using MediaService.Application.Storage;
using MediaService.Domain;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace MediaService.Infrastructure.Persistence;

public sealed class EfMediaRepository(
    MediaDbContext dbContext,
    TimeProvider timeProvider) : IMediaRepository
{
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
                item.DeletedAt))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<MediaUsageRecord> ReplaceStudentAvatarAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var activeUsages = await dbContext.MediaUsages
            .Where(item =>
                item.OwnerService == MediaOwnerServices.Student &&
                item.OwnerType == MediaOwnerTypes.StudentAvatar &&
                item.UsageType == MediaUsageTypes.Avatar &&
                item.OwnerId == usage.OwnerId &&
                item.DeletedAt == null)
            .ToListAsync(cancellationToken);

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
            throw new MediaApplicationException(
                MediaErrorCodes.MediaUsageConflict,
                "The media usage conflicts with an active usage.",
                409);
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
}
