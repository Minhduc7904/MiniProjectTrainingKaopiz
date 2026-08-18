// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Repositories/EfNotificationBatchRepository.cs
// Mục đích: Lưu/đọc batch, snapshot recipient, claim chunk bằng lease và áp dụng Domain transition trong transaction EF Core.

using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.UseCases.NotificationBatches;
using NotificationService.Domain.Constants;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence.Context;
using NotificationService.Infrastructure.Persistence.Mappers;
using NotificationService.Infrastructure.Persistence.Scaffolded;
using System.Data;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class EfNotificationBatchRepository(
    NotificationDbContext dbContext,
    IDbContextFactory<NotificationDbContext> dbContextFactory,
    TimeProvider timeProvider,
    NotificationBatchProcessingOptions processingOptions) :
        INotificationBatchRepository,
        INotificationBatchDispatchRepository
{
    public Task<NotificationBatchSummary> CreateAsync(CreateNotificationBatchRecord record, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var batch = new NotificationBatch { Id = record.Id, Title = record.Title, BodyMarkdown = record.BodyMarkdown, TargetScope = NotificationTargetScopes.AllStudents, CreatedBy = record.CreatedBy, Status = NotificationBatchStatuses.Pending, TotalCount = 0, BatchSize = record.BatchSize, CreatedAt = record.CreatedAtUtc };
        dbContext.NotificationBatches.Add(batch);
        return Task.FromResult(NotificationBatchPersistenceMapper.ToSummary(batch));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<NotificationSnapshotWork> PrepareSnapshotAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.SingleOrDefaultAsync(
            x => x.Id == batchId,
            cancellationToken);
        if (batch is null)
        {
            return new NotificationSnapshotWork(false, false);
        }
        var state = NotificationBatchPersistenceMapper.ToDomain(batch);
        var decision = state.PrepareSnapshot();
        NotificationBatchPersistenceMapper.Apply(state, batch);
        if (decision == NotificationSnapshotDecision.ReadRecipients)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        return new NotificationSnapshotWork(
            decision == NotificationSnapshotDecision.ReadRecipients,
            decision == NotificationSnapshotDecision.Dispatch);
    }

    public async Task AppendSnapshotPageAsync(
        Guid batchId,
        IReadOnlyList<Guid> studentIds,
        CancellationToken cancellationToken)
    {
        if (studentIds.Count == 0)
        {
            return;
        }

        var batchStatus = await dbContext.NotificationBatches
            .Where(x => x.Id == batchId)
            .Select(x => x.Status)
            .SingleOrDefaultAsync(cancellationToken);
        if (batchStatus != NotificationBatchStatuses.Snapshotting)
        {
            return;
        }

        var parameters = new List<MySqlParameter>();
        var values = new List<string>();
        for (var index = 0; index < studentIds.Count; index++)
        {
            var idParameter = new MySqlParameter($"@id{index}", Guid.NewGuid());
            var batchParameter = new MySqlParameter($"@batchId{index}", batchId);
            var studentParameter = new MySqlParameter($"@studentId{index}", studentIds[index]);
            parameters.AddRange([idParameter, batchParameter, studentParameter]);
            values.Add($"(@id{index}, @batchId{index}, @studentId{index}, '{NotificationBatchItemStatuses.Pending}', 0)");
        }

        var sql = $"""
            INSERT INTO notification_batch_items (id, batch_id, student_id, status, retry_count)
            VALUES {string.Join(", ", values)}
            ON DUPLICATE KEY UPDATE id = id;
            """;
        await dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    public async Task<bool> CompleteSnapshotAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.SingleAsync(x => x.Id == batchId, cancellationToken);
        var totalCount = checked((uint)await dbContext.NotificationBatchItems
            .CountAsync(x => x.BatchId == batchId, cancellationToken));
        var state = NotificationBatchPersistenceMapper.ToDomain(batch);
        var shouldDispatch = state.CompleteSnapshot(totalCount, DateTime.UtcNow);
        NotificationBatchPersistenceMapper.Apply(state, batch);
        await dbContext.SaveChangesAsync(cancellationToken);
        return shouldDispatch;
    }

    public async Task MarkSnapshotFailedAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.SingleOrDefaultAsync(x => x.Id == batchId, cancellationToken);
        if (batch is null)
        {
            return;
        }
        var state = NotificationBatchPersistenceMapper.ToDomain(batch);
        state.MarkSnapshotFailed(DateTime.UtcNow);
        NotificationBatchPersistenceMapper.Apply(state, batch);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken) =>
        await dbContext.NotificationBatches.AsNoTracking().Where(x => x.Id == batchId).Select(x => new NotificationBatchSummary(x.Id, x.Status, x.TotalCount, x.ProcessedCount, x.SuccessCount, x.FailedCount, x.BatchSize, x.CreatedAt, x.StartedAt, x.CompletedAt)).SingleOrDefaultAsync(cancellationToken);

    public async Task<NotificationBatchFailedItemsPage> GetFailedItemsAsync(
        Guid batchId,
        Guid? afterItemId,
        int limit,
        CancellationToken cancellationToken)
    {
        IQueryable<NotificationBatchItem> items = dbContext.NotificationBatchItems
            .AsNoTracking()
            .Where(x => x.BatchId == batchId && x.Status == NotificationBatchItemStatuses.Failed);

        if (afterItemId is not null)
        {
            items = items.Where(x => x.Id.CompareTo(afterItemId.Value) > 0);
        }

        var rows = await items
            .OrderBy(x => x.Id)
            .Take(limit + 1)
            .Select(x => new { x.Id, x.StudentId, x.RetryCount, x.ErrorMessage })
            .ToListAsync(cancellationToken);
        var hasNextPage = rows.Count > limit;
        var page = rows.Take(limit).ToArray();

        return new NotificationBatchFailedItemsPage(
            page.Select(x => new NotificationBatchFailedItem(
                x.StudentId,
                x.RetryCount,
                x.ErrorMessage ?? "Unknown sender error.")).ToArray(),
            hasNextPage ? page[^1].Id : null,
            hasNextPage);
    }

    public async Task<NotificationBatchClaim?> ClaimChunkAsync(Guid batchId, CancellationToken cancellationToken)
    {
        await using var claimContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await claimContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        var batch = await claimContext.NotificationBatches.SingleOrDefaultAsync(
            x => x.Id == batchId,
            cancellationToken);
        if (batch is null || batch.Status is not (NotificationBatchStatuses.SnapshotReady or NotificationBatchStatuses.Processing))
        {
            return null;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var leaseToken = Guid.NewGuid();
        var items = await claimContext.NotificationBatchItems
            .FromSqlInterpolated($"""
                SELECT *
                FROM notification_batch_items
                WHERE batch_id = {batchId}
                  AND (
                    status IN ({NotificationBatchItemStatuses.Pending}, {NotificationBatchItemStatuses.Retry})
                    OR (status = {NotificationBatchItemStatuses.Processing}
                        AND lease_expires_at IS NOT NULL
                        AND lease_expires_at <= {now})
                  )
                ORDER BY id
                LIMIT {checked((int)batch.BatchSize)}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);
        if (items.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        var batchState = NotificationBatchPersistenceMapper.ToDomain(batch);
        batchState.StartProcessing(now);
        NotificationBatchPersistenceMapper.Apply(batchState, batch);
        var leaseExpiresAt = now.AddSeconds(processingOptions.ClaimLeaseSeconds);
        foreach (var item in items)
        {
            item.Status = NotificationBatchItemStatuses.Processing;
            item.LeaseToken = leaseToken;
            item.LeaseExpiresAt = leaseExpiresAt;
        }

        await claimContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new NotificationBatchClaim(
            batchId,
            leaseToken,
            items.Select(item => new NotificationBatchWorkItem(
                item.Id,
                item.BatchId,
                item.StudentId,
                item.RetryCount,
                batch.Title,
                batch.BodyMarkdown,
                batch.CreatedBy)).ToArray());
    }

    public async Task<IReadOnlyList<NotificationSummary>> CompleteClaimAsync(
        NotificationBatchClaim claim,
        IReadOnlyList<NotificationBatchDeliveryResult> results,
        CancellationToken cancellationToken)
    {
        var resultByItemId = results
            .GroupBy(result => result.ItemId)
            .ToDictionary(group => group.Key, group => group.Single());
        if (resultByItemId.Count != claim.Items.Count ||
            claim.Items.Any(item => !resultByItemId.ContainsKey(item.Id)))
        {
            throw new InvalidOperationException("Every claimed notification batch item must have exactly one delivery result.");
        }

        var itemIds = claim.Items.Select(item => item.Id).ToArray();
        var batch = await dbContext.NotificationBatches.SingleAsync(
            batch => batch.Id == claim.BatchId,
            cancellationToken);
        var claimedItems = await dbContext.NotificationBatchItems
            .Where(item =>
                item.BatchId == claim.BatchId &&
                item.Status == NotificationBatchItemStatuses.Processing &&
                item.LeaseToken == claim.LeaseToken &&
                itemIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
        if (claimedItems.Count != claim.Items.Count)
        {
            throw new InvalidOperationException("The notification batch claim has expired or was released before completion.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var notifications = new List<NotificationSummary>();
        uint successCount = 0;
        uint failedCount = 0;
        foreach (var item in claimedItems)
        {
            var result = resultByItemId[item.Id];
            item.LeaseToken = null;
            item.LeaseExpiresAt = null;
            if (result.IsSuccess)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientStudentId = item.StudentId,
                    Title = batch.Title,
                    BodyMarkdown = batch.BodyMarkdown,
                    SourceType = NotificationSourceTypes.Bulk,
                    NotificationBatchId = item.BatchId,
                    CreatedBy = batch.CreatedBy,
                    Status = NotificationStatuses.Unread,
                    CreatedAt = now,
                };
                dbContext.Notifications.Add(notification);
                var successfulItemState = NotificationBatchItemPersistenceMapper.ToDomain(item);
                successfulItemState.ApplyDeliveryResult(true, null, now);
                NotificationBatchItemPersistenceMapper.Apply(successfulItemState, item);
                item.NotificationId = notification.Id;
                successCount++;
                notifications.Add(new NotificationSummary(
                    notification.Id,
                    notification.RecipientStudentId,
                    notification.Title,
                    notification.BodyMarkdown,
                    notification.SourceType,
                    notification.Status,
                    notification.CreatedBy,
                    notification.CreatedAt,
                    notification.ReadAt));
                continue;
            }

            var itemState = NotificationBatchItemPersistenceMapper.ToDomain(item);
            itemState.ApplyDeliveryResult(false, result.ErrorMessage, now);
            NotificationBatchItemPersistenceMapper.Apply(itemState, item);
            if (itemState.Status == NotificationBatchItemStatuses.Failed)
            {
                failedCount++;
            }
        }
        var completedBatchState = NotificationBatchPersistenceMapper.ToDomain(batch);
        completedBatchState.AddDeliveryCounts(successCount, failedCount);
        NotificationBatchPersistenceMapper.Apply(completedBatchState, batch);
        await dbContext.SaveChangesAsync(cancellationToken);
        return notifications;
    }

    public async Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.SingleAsync(x => x.Id == batchId, cancellationToken);
        var hasPending = await dbContext.NotificationBatchItems.AnyAsync(
            item => item.BatchId == batchId &&
                    (item.Status == NotificationBatchItemStatuses.Pending ||
                     item.Status == NotificationBatchItemStatuses.Retry),
            cancellationToken);
        if (hasPending)
        {
            return true;
        }

        var hasActiveClaim = await dbContext.NotificationBatchItems.AnyAsync(
            item => item.BatchId == batchId &&
                    item.Status == NotificationBatchItemStatuses.Processing,
            cancellationToken);
        if (hasActiveClaim)
        {
            return false;
        }

        var state = NotificationBatchPersistenceMapper.ToDomain(batch);
        var hasRemaining = state.FinalizeOrHasRemaining(
            false, false, timeProvider.GetUtcNow().UtcDateTime);
        NotificationBatchPersistenceMapper.Apply(state, batch);
        await dbContext.SaveChangesAsync(cancellationToken);
        return hasRemaining;
    }
}
