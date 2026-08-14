using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Persistence.Scaffolded;

namespace NotificationService.Infrastructure.Persistence;

public sealed class EfNotificationBatchRepository(NotificationDbContext dbContext) : INotificationBatchRepository
{
    public Task<NotificationBatchSummary> CreateAsync(CreateNotificationBatchRecord record, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var batch = new NotificationBatch { Id = record.Id, Title = record.Title, BodyMarkdown = record.BodyMarkdown, TargetScope = NotificationTargetScopes.AllStudents, CreatedBy = record.CreatedBy, Status = NotificationBatchStatuses.Pending, TotalCount = 0, BatchSize = record.BatchSize, CreatedAt = record.CreatedAtUtc };
        dbContext.NotificationBatches.Add(batch);
        return Task.FromResult(ToSummary(batch));
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
        if (batch is null || batch.Status is NotificationBatchStatuses.Completed or NotificationBatchStatuses.PartialFailed or NotificationBatchStatuses.Failed)
        {
            return new NotificationSnapshotWork(false, false);
        }

        if (batch.Status == NotificationBatchStatuses.SnapshotReady)
        {
            return new NotificationSnapshotWork(false, true);
        }

        batch.Status = NotificationBatchStatuses.Snapshotting;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new NotificationSnapshotWork(true, false);
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
        if (batch.Status != NotificationBatchStatuses.Snapshotting)
        {
            return batch.Status == NotificationBatchStatuses.SnapshotReady;
        }

        batch.TotalCount = checked((uint)await dbContext.NotificationBatchItems
            .CountAsync(x => x.BatchId == batchId, cancellationToken));
        if (batch.TotalCount == 0)
        {
            batch.Status = NotificationBatchStatuses.Failed;
            batch.CompletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        batch.Status = NotificationBatchStatuses.SnapshotReady;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task MarkSnapshotFailedAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.SingleOrDefaultAsync(x => x.Id == batchId, cancellationToken);
        if (batch is null || batch.Status is NotificationBatchStatuses.Completed or NotificationBatchStatuses.PartialFailed or NotificationBatchStatuses.Failed)
        {
            return;
        }

        batch.Status = NotificationBatchStatuses.Failed;
        batch.CompletedAt = DateTime.UtcNow;
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

    public async Task<IReadOnlyList<NotificationBatchWorkItem>> ClaimChunkAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.SingleOrDefaultAsync(x => x.Id == batchId, cancellationToken);
        if (batch is null || batch.Status is not (NotificationBatchStatuses.SnapshotReady or NotificationBatchStatuses.Processing)) return [];
        var items = await dbContext.NotificationBatchItems
            .Where(x =>
                x.BatchId == batchId &&
                (x.Status == NotificationBatchItemStatuses.Pending ||
                 x.Status == NotificationBatchItemStatuses.Retry ||
                 x.Status == NotificationBatchItemStatuses.Processing))
            .OrderBy(x => x.Id)
            .Take(checked((int)batch.BatchSize))
            .ToListAsync(cancellationToken);
        if (items.Count == 0) return [];
        batch.Status = NotificationBatchStatuses.Processing;
        batch.StartedAt ??= DateTime.UtcNow;
        foreach (var item in items) item.Status = NotificationBatchItemStatuses.Processing;
        await dbContext.SaveChangesAsync(cancellationToken);
        return items.Select(x => new NotificationBatchWorkItem(x.Id, x.BatchId, x.StudentId, x.RetryCount, batch.Title, batch.BodyMarkdown, batch.CreatedBy)).ToArray();
    }

    public async Task<NotificationSummary?> MarkSuccessAsync(NotificationBatchWorkItem item, CancellationToken cancellationToken)
    {
        var entity = await dbContext.NotificationBatchItems.SingleAsync(x => x.Id == item.Id, cancellationToken);
        if (entity.Status is NotificationBatchItemStatuses.Success or NotificationBatchItemStatuses.Failed) return null;
        var notification = new Notification { Id = Guid.NewGuid(), RecipientStudentId = item.StudentId, Title = item.Title, BodyMarkdown = item.BodyMarkdown, SourceType = NotificationSourceTypes.Bulk, NotificationBatchId = item.BatchId, CreatedBy = item.CreatedBy, Status = NotificationStatuses.Unread, CreatedAt = DateTime.UtcNow };
        dbContext.Notifications.Add(notification);
        entity.Status = NotificationBatchItemStatuses.Success;
        entity.NotificationId = notification.Id;
        entity.ProcessedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new NotificationSummary(notification.Id, notification.RecipientStudentId, notification.Title, notification.BodyMarkdown, notification.SourceType, notification.Status, notification.CreatedBy, notification.CreatedAt, notification.ReadAt);
    }

    public async Task MarkFailureAsync(NotificationBatchWorkItem item, string errorMessage, CancellationToken cancellationToken)
    {
        var entity = await dbContext.NotificationBatchItems.SingleAsync(x => x.Id == item.Id, cancellationToken);
        if (entity.Status is NotificationBatchItemStatuses.Success or NotificationBatchItemStatuses.Failed) return;
        entity.RetryCount++;
        entity.Status = entity.RetryCount >= 2 ? NotificationBatchItemStatuses.Failed : NotificationBatchItemStatuses.Retry;
        entity.ErrorMessage = errorMessage;
        if (entity.Status == NotificationBatchItemStatuses.Failed) entity.ProcessedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await dbContext.NotificationBatches.SingleAsync(x => x.Id == batchId, cancellationToken);
        var items = dbContext.NotificationBatchItems.Where(x => x.BatchId == batchId);
        var hasRemaining = await items.AnyAsync(
            x => x.Status == NotificationBatchItemStatuses.Pending || x.Status == NotificationBatchItemStatuses.Retry || x.Status == NotificationBatchItemStatuses.Processing,
            cancellationToken);
        batch.SuccessCount = checked((uint)await items.CountAsync(
            x => x.Status == NotificationBatchItemStatuses.Success,
            cancellationToken));
        batch.FailedCount = checked((uint)await items.CountAsync(
            x => x.Status == NotificationBatchItemStatuses.Failed,
            cancellationToken));
        batch.ProcessedCount = batch.SuccessCount + batch.FailedCount;
        if (hasRemaining)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        batch.Status = batch.FailedCount == 0 ? NotificationBatchStatuses.Completed : batch.SuccessCount == 0 ? NotificationBatchStatuses.Failed : NotificationBatchStatuses.PartialFailed;
        batch.CompletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return false;
    }

    private static NotificationBatchSummary ToSummary(NotificationBatch x) => new(x.Id, x.Status, x.TotalCount, x.ProcessedCount, x.SuccessCount, x.FailedCount, x.BatchSize, x.CreatedAt, x.StartedAt, x.CompletedAt);
}
