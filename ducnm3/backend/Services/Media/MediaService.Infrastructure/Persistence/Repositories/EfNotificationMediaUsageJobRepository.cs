// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Repositories/EfNotificationMediaUsageJobRepository.cs
// Mục đích: Lưu tiến độ Media Usage job bằng atomic SQL để nhiều chunk không ghi đè counter của nhau.

using MediaService.Application.Repositories;
using MediaService.Domain.Constants;
using MediaService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence.Repositories;

public sealed class EfNotificationMediaUsageJobRepository(MediaDbContext dbContext)
    : INotificationMediaUsageJobRepository
{
    public Task<NotificationMediaUsageJobRecord?> GetByIdAsync(
        Guid jobId,
        CancellationToken cancellationToken) =>
        dbContext.NotificationMediaUsageJobs.AsNoTracking()
            .Where(job => job.Id == jobId)
            .Select(job => new NotificationMediaUsageJobRecord(
                job.Id, job.Status, job.ExpectedUsageCount, job.ProcessedUsageCount,
                job.FailedUsageCount, job.CreatedAt, job.StartedAt,
                job.CompletedAt, job.LastError))
            .SingleOrDefaultAsync(cancellationToken);

    public Task StartAsync(Guid jobId, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO notification_media_usage_jobs (id, status)
            VALUES ({jobId}, {NotificationMediaUsageJobStatuses.Pending})
            ON DUPLICATE KEY UPDATE id = VALUES(id);
            """, cancellationToken);

    public async Task RecordSuccessAsync(
        Guid jobId,
        uint usageCount,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO notification_media_usage_jobs
                (id, status, processed_usage_count, started_at)
            VALUES ({jobId}, {NotificationMediaUsageJobStatuses.Processing}, {usageCount}, UTC_TIMESTAMP(6))
            ON DUPLICATE KEY UPDATE
                processed_usage_count = processed_usage_count + VALUES(processed_usage_count),
                started_at = COALESCE(started_at, VALUES(started_at)),
                status = {NotificationMediaUsageJobStatuses.Processing};
            """, cancellationToken);
        await FinalizeIfReadyAsync(jobId, cancellationToken);
    }

    public async Task RecordFailureAsync(
        Guid jobId,
        uint usageCount,
        string safeError,
        CancellationToken cancellationToken)
    {
        safeError = string.IsNullOrWhiteSpace(safeError)
            ? "Media Usage processing failed."
            : safeError[..Math.Min(safeError.Length, 500)];
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO notification_media_usage_jobs
                (id, status, failed_usage_count, last_error, started_at)
            VALUES ({jobId}, {NotificationMediaUsageJobStatuses.Processing}, {usageCount}, {safeError}, UTC_TIMESTAMP(6))
            ON DUPLICATE KEY UPDATE
                failed_usage_count = failed_usage_count + VALUES(failed_usage_count),
                last_error = VALUES(last_error),
                started_at = COALESCE(started_at, VALUES(started_at)),
                status = {NotificationMediaUsageJobStatuses.Processing};
            """, cancellationToken);
        await FinalizeIfReadyAsync(jobId, cancellationToken);
    }

    public async Task CompleteSourceAsync(
        Guid jobId,
        uint expectedUsageCount,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO notification_media_usage_jobs
                (id, status, expected_usage_count)
            VALUES ({jobId}, {NotificationMediaUsageJobStatuses.Pending}, {expectedUsageCount})
            ON DUPLICATE KEY UPDATE expected_usage_count = VALUES(expected_usage_count);
            """, cancellationToken);
        await FinalizeIfReadyAsync(jobId, cancellationToken);
    }

    private Task<int> FinalizeIfReadyAsync(Guid jobId, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE notification_media_usage_jobs
            SET status = CASE
                    WHEN failed_usage_count = 0 THEN {NotificationMediaUsageJobStatuses.Completed}
                    WHEN processed_usage_count = 0 THEN {NotificationMediaUsageJobStatuses.Failed}
                    ELSE {NotificationMediaUsageJobStatuses.PartialFailed}
                END,
                completed_at = COALESCE(completed_at, UTC_TIMESTAMP(6))
            WHERE id = {jobId}
              AND expected_usage_count IS NOT NULL
              AND processed_usage_count + failed_usage_count >= expected_usage_count;
            """, cancellationToken);
}
