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
        var batch = new NotificationBatch { Id = record.Id, Title = record.Title, BodyMarkdown = record.BodyMarkdown, TargetScope = record.TargetScope, CreatedBy = record.CreatedBy, Status = NotificationBatchStatuses.Pending, TotalCount = 0, BatchSize = record.BatchSize, RequestedCount = record.RequestedCount, SourceBatchId = record.SourceBatchId, CreatedAt = record.CreatedAtUtc };
        dbContext.NotificationBatches.Add(batch);
        return Task.FromResult(NotificationBatchPersistenceMapper.ToSummary(batch));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<NotificationBatchRetryCreation> PrepareRetryAsync(
        Guid sourceBatchId,
        Guid createdBy,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.NotificationBatches.AsNoTracking()
            .SingleOrDefaultAsync(x => x.SourceBatchId == sourceBatchId, cancellationToken);
        if (existing is not null)
        {
            return new NotificationBatchRetryCreation(
                NotificationBatchPersistenceMapper.ToSummary(existing), false);
        }

        var source = await dbContext.NotificationBatches.AsNoTracking()
            .SingleAsync(x => x.Id == sourceBatchId, cancellationToken);
        var retry = new NotificationBatch
        {
            Id = Guid.NewGuid(),
            Title = source.Title,
            BodyMarkdown = source.BodyMarkdown,
            TargetScope = NotificationTargetScopes.FailedRecipients,
            CreatedBy = createdBy,
            Status = NotificationBatchStatuses.Pending,
            TotalCount = 0,
            BatchSize = source.BatchSize,
            RequestedCount = source.FailedCount,
            SourceBatchId = sourceBatchId,
            CreatedAt = createdAtUtc,
        };
        dbContext.NotificationBatches.Add(retry);
        return new NotificationBatchRetryCreation(
            NotificationBatchPersistenceMapper.ToSummary(retry), true);
    }

    public async Task<NotificationBatchSummary> CommitRetryAsync(
        Guid sourceBatchId,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            var committed = dbContext.NotificationBatches.Local
                .Single(x => x.SourceBatchId == sourceBatchId);
            return NotificationBatchPersistenceMapper.ToSummary(committed);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1062 })
        {
            dbContext.ChangeTracker.Clear();
            var winner = await dbContext.NotificationBatches.AsNoTracking()
                .SingleAsync(x => x.SourceBatchId == sourceBatchId, cancellationToken);
            return NotificationBatchPersistenceMapper.ToSummary(winner);
        }
    }

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
        var existingRecipientCount = decision == NotificationSnapshotDecision.ReadRecipients
            ? checked((uint)await dbContext.NotificationBatchItems
                .CountAsync(item => item.BatchId == batchId, cancellationToken))
            : 0;
        return new NotificationSnapshotWork(
            decision == NotificationSnapshotDecision.ReadRecipients,
            decision == NotificationSnapshotDecision.Dispatch,
            batch.RequestedCount,
            batch.SourceBatchId,
            existingRecipientCount);
    }

    public async Task<int> AppendSnapshotPageAsync(
        Guid batchId,
        IReadOnlyList<Guid> studentIds,
        CancellationToken cancellationToken)
    {
        if (studentIds.Count == 0)
        {
            return 0;
        }

        var batchStatus = await dbContext.NotificationBatches
            .Where(x => x.Id == batchId)
            .Select(x => x.Status)
            .SingleOrDefaultAsync(cancellationToken);
        if (batchStatus != NotificationBatchStatuses.Snapshotting)
        {
            return 0;
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
            INSERT IGNORE INTO notification_batch_items (id, batch_id, student_id, status, retry_count)
            VALUES {string.Join(", ", values)}
            """;
        return await dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    public Task CopyFailedRecipientsAsync(
        Guid batchId,
        Guid sourceBatchId,
        CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO notification_batch_items (id, batch_id, student_id, status, retry_count)
            SELECT UUID(), {batchId}, student_id, {NotificationBatchItemStatuses.Pending}, 0
            FROM notification_batch_items
            WHERE batch_id = {sourceBatchId} AND status = {NotificationBatchItemStatuses.Failed}
            ORDER BY id
            ON DUPLICATE KEY UPDATE student_id = VALUES(student_id);
            """, cancellationToken);

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

    public async Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.NotificationBatches.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == batchId, cancellationToken);
        return entity is null ? null : NotificationBatchPersistenceMapper.ToSummary(entity);
    }

    public async Task<NotificationBatchSnapshotProgress?> GetSnapshotProgressAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.AsNoTracking()
            .Where(x => x.Id == batchId)
            .Select(x => new { x.Id, x.Status, x.RequestedCount, x.TotalCount })
            .SingleOrDefaultAsync(cancellationToken);
        if (batch is null)
        {
            return null;
        }

        var snapshotCount = checked((uint)await dbContext.NotificationBatchItems.AsNoTracking()
            .CountAsync(x => x.BatchId == batchId, cancellationToken));
        uint? totalCount = batch.Status is NotificationBatchStatuses.Pending or NotificationBatchStatuses.Snapshotting
            ? null
            : batch.TotalCount;
        return new NotificationBatchSnapshotProgress(
            batch.Id,
            batch.Status,
            batch.RequestedCount,
            snapshotCount,
            totalCount);
    }

    public async Task<NotificationBatchListPage> ListAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.NotificationBatches.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var totalItems = await query.LongCountAsync(cancellationToken);
        var entities = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new NotificationBatchListPage(
            entities.Select(NotificationBatchPersistenceMapper.ToSummary).ToArray(),
            page, pageSize, totalItems);
    }

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

        var batch = await dbContext.NotificationBatches.SingleAsync(
            batch => batch.Id == claim.BatchId,
            cancellationToken);
        var claimedItems = await dbContext.NotificationBatchItems
            .Where(item =>
                item.BatchId == claim.BatchId &&
                item.Status == NotificationBatchItemStatuses.Processing &&
                item.LeaseToken == claim.LeaseToken)
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

    public async Task<NotificationBatchContinuation> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.SingleAsync(x => x.Id == batchId, cancellationToken);
        var hasPending = await dbContext.NotificationBatchItems.AnyAsync(
            item => item.BatchId == batchId &&
                    (item.Status == NotificationBatchItemStatuses.Pending ||
                     item.Status == NotificationBatchItemStatuses.Retry),
            cancellationToken);
        if (hasPending)
        {
            return new NotificationBatchContinuation(true, false, batch.SuccessCount, batch.BodyMarkdown);
        }

        var hasActiveClaim = await dbContext.NotificationBatchItems.AnyAsync(
            item => item.BatchId == batchId &&
                    item.Status == NotificationBatchItemStatuses.Processing,
            cancellationToken);
        if (hasActiveClaim)
        {
            return new NotificationBatchContinuation(false, false, batch.SuccessCount, batch.BodyMarkdown);
        }

        var state = NotificationBatchPersistenceMapper.ToDomain(batch);
        var hasRemaining = state.FinalizeOrHasRemaining(
            false, false, timeProvider.GetUtcNow().UtcDateTime);
        NotificationBatchPersistenceMapper.Apply(state, batch);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new NotificationBatchContinuation(
            hasRemaining,
            !hasRemaining,
            batch.SuccessCount,
            batch.BodyMarkdown);
    }
}
