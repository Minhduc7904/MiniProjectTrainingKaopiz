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
    private const string NotificationBatchSubjectType = "NOTIFICATION_BATCH";
    private const string EmptyPayload = "{}";
    public Task<NotificationMediaUsageJobRecord?> GetByIdAsync(
        Guid jobId,
        CancellationToken cancellationToken) =>
        dbContext.MediaBackgroundJobs.AsNoTracking()
            .Where(job => job.Id == jobId && job.JobType == MediaBackgroundJobTypes.NotificationUsage)
            .Select(job => new NotificationMediaUsageJobRecord(
                job.Id, job.Status, job.ExpectedItemCount, job.ProcessedItemCount,
                job.FailedItemCount, job.CreatedAt, job.StartedAt,
                job.CompletedAt, job.LastError))
            .SingleOrDefaultAsync(cancellationToken);

    public Task StartAsync(Guid jobId, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO media_background_jobs
                (id, job_type, subject_type, subject_id, correlation_id, status, payload_json)
            VALUES ({jobId}, {MediaBackgroundJobTypes.NotificationUsage}, {NotificationBatchSubjectType}, {jobId}, {jobId}, {MediaBackgroundJobStatuses.Queued}, {EmptyPayload})
            ON DUPLICATE KEY UPDATE id = VALUES(id);
            """, cancellationToken);

    public async Task RecordSuccessAsync(
        Guid jobId,
        uint usageCount,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO media_background_jobs
                (id, job_type, subject_type, subject_id, correlation_id, status, payload_json, processed_item_count, started_at)
            VALUES ({jobId}, {MediaBackgroundJobTypes.NotificationUsage}, {NotificationBatchSubjectType}, {jobId}, {jobId}, {MediaBackgroundJobStatuses.Processing}, {EmptyPayload}, {usageCount}, UTC_TIMESTAMP(6))
            ON DUPLICATE KEY UPDATE
                processed_item_count = processed_item_count + VALUES(processed_item_count),
                started_at = COALESCE(started_at, VALUES(started_at)),
                status = {MediaBackgroundJobStatuses.Processing};
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
            INSERT INTO media_background_jobs
                (id, job_type, subject_type, subject_id, correlation_id, status, payload_json, failed_item_count, last_error, started_at)
            VALUES ({jobId}, {MediaBackgroundJobTypes.NotificationUsage}, {NotificationBatchSubjectType}, {jobId}, {jobId}, {MediaBackgroundJobStatuses.Processing}, {EmptyPayload}, {usageCount}, {safeError}, UTC_TIMESTAMP(6))
            ON DUPLICATE KEY UPDATE
                failed_item_count = failed_item_count + VALUES(failed_item_count),
                last_error = VALUES(last_error),
                started_at = COALESCE(started_at, VALUES(started_at)),
                status = {MediaBackgroundJobStatuses.Processing};
            """, cancellationToken);
        await FinalizeIfReadyAsync(jobId, cancellationToken);
    }

    public async Task CompleteSourceAsync(
        Guid jobId,
        uint expectedUsageCount,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO media_background_jobs
                (id, job_type, subject_type, subject_id, correlation_id, status, payload_json, expected_item_count)
            VALUES ({jobId}, {MediaBackgroundJobTypes.NotificationUsage}, {NotificationBatchSubjectType}, {jobId}, {jobId}, {MediaBackgroundJobStatuses.Queued}, {EmptyPayload}, {expectedUsageCount})
            ON DUPLICATE KEY UPDATE expected_item_count = VALUES(expected_item_count);
            """, cancellationToken);
        await FinalizeIfReadyAsync(jobId, cancellationToken);
    }

    private Task<int> FinalizeIfReadyAsync(Guid jobId, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE media_background_jobs
            SET status = CASE
                    WHEN failed_item_count = 0 THEN {MediaBackgroundJobStatuses.Completed}
                    WHEN processed_item_count = 0 THEN {MediaBackgroundJobStatuses.Failed}
                    ELSE {MediaBackgroundJobStatuses.PartialFailed}
                END,
                completed_at = COALESCE(completed_at, UTC_TIMESTAMP(6))
            WHERE id = {jobId}
              AND expected_item_count IS NOT NULL
              AND processed_item_count + failed_item_count >= expected_item_count;
            """, cancellationToken);
}
