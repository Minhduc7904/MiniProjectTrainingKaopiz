// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Repositories/EfMediaBackgroundJobRepository.cs
// Mục đích: Tập trung mọi truy cập bảng media_background_jobs cho các repository Media khác.

using MediaService.Domain.Constants;
using MediaService.Application.Repositories;
using MediaService.Application.UseCases.MediaUsageJobs.GetList;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence.Repositories;

public sealed class EfMediaBackgroundJobRepository(MediaDbContext dbContext)
    : IMediaBackgroundJobListRepository, IMediaBackgroundJobLifecycleRepository
{
    public Task<MediaBackgroundJob?> GetThumbnailTrackedAsync(Guid jobId, Guid sourceMediaId, CancellationToken cancellationToken) =>
        dbContext.MediaBackgroundJobs.SingleOrDefaultAsync(job => job.Id == jobId &&
            job.JobType == MediaBackgroundJobTypes.ThumbnailDerivation && job.SubjectId == sourceMediaId,
            cancellationToken);

    public Task<MediaBackgroundJob?> GetThumbnailByIdAsync(Guid jobId, CancellationToken cancellationToken) =>
        dbContext.MediaBackgroundJobs.SingleOrDefaultAsync(job => job.Id == jobId &&
            job.JobType == MediaBackgroundJobTypes.ThumbnailDerivation, cancellationToken);

    public Task<MediaBackgroundJob?> GetThumbnailBySourceAsync(Guid sourceMediaId, bool tracked, CancellationToken cancellationToken)
    {
        IQueryable<MediaBackgroundJob> query = dbContext.MediaBackgroundJobs;
        if (!tracked) query = query.AsNoTracking();
        return query.SingleOrDefaultAsync(job => job.JobType == MediaBackgroundJobTypes.ThumbnailDerivation && job.SubjectId == sourceMediaId, cancellationToken);
    }

    public void Add(MediaBackgroundJob job) => dbContext.MediaBackgroundJobs.Add(job);

    public Task StartAsync(
        Guid jobId,
        string jobType,
        string subjectType,
        Guid subjectId,
        Guid? correlationId,
        uint expectedItemCount,
        string payloadJson,
        CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO media_background_jobs
                (id, job_type, subject_type, subject_id, correlation_id, status, payload_json,
                 expected_item_count, attempt_count, started_at)
            VALUES ({jobId}, {jobType}, {subjectType}, {subjectId}, {correlationId},
                    {MediaBackgroundJobStatuses.Processing}, {payloadJson}, {expectedItemCount}, 1, UTC_TIMESTAMP(6))
            ON DUPLICATE KEY UPDATE
                attempt_count = attempt_count + 1,
                started_at = COALESCE(started_at, UTC_TIMESTAMP(6)),
                status = IF(status = {MediaBackgroundJobStatuses.Completed}, status, {MediaBackgroundJobStatuses.Processing}),
                updated_at = UTC_TIMESTAMP(6),
                last_error = NULL;
            """, cancellationToken);

    public Task CompleteAsync(Guid jobId, uint processedItemCount, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE media_background_jobs
            SET status = {MediaBackgroundJobStatuses.Completed},
                processed_item_count = {processedItemCount},
                failed_item_count = 0,
                completed_at = UTC_TIMESTAMP(6),
                updated_at = UTC_TIMESTAMP(6),
                last_error = NULL
            WHERE id = {jobId};
            """, cancellationToken);

    public Task FailAsync(Guid jobId, uint failedItemCount, string safeError, CancellationToken cancellationToken)
    {
        var error = string.IsNullOrWhiteSpace(safeError)
            ? "Media background job failed."
            : safeError[..Math.Min(safeError.Length, 500)];
        return dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE media_background_jobs
            SET status = {MediaBackgroundJobStatuses.Failed},
                failed_item_count = {failedItemCount},
                completed_at = UTC_TIMESTAMP(6),
                updated_at = UTC_TIMESTAMP(6),
                last_error = {error}
            WHERE id = {jobId} AND status <> {MediaBackgroundJobStatuses.Completed};
            """, cancellationToken);
    }

    public IQueryable<MediaBackgroundJob> Query() => dbContext.MediaBackgroundJobs;

    public async Task<MediaBackgroundJobListPage> ListAsync(
        MediaBackgroundJobListRequest request,
        CancellationToken cancellationToken)
    {
        IQueryable<MediaBackgroundJob> query = dbContext.MediaBackgroundJobs.AsNoTracking();
        if (request.JobType is not null) query = query.Where(job => job.JobType == request.JobType);
        if (request.Status is not null) query = query.Where(job => job.Status == request.Status);
        if (request.CorrelationId is { } correlationId) query = query.Where(job => job.CorrelationId == correlationId);

        var totalItems = await query.LongCountAsync(cancellationToken);
        var skip = checked((request.Page - 1) * request.PageSize);
        var items = await query
            .OrderByDescending(job => job.UpdatedAt)
            .ThenByDescending(job => job.Id)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(job => new MediaBackgroundJobListRecord(
                job.Id, job.JobType, job.SubjectType, job.SubjectId, job.CorrelationId,
                job.Status, job.ExpectedItemCount, job.ProcessedItemCount,
                job.FailedItemCount, job.AttemptCount, job.LastError, job.CreatedAt,
                job.StartedAt, job.CompletedAt, job.UpdatedAt))
            .ToListAsync(cancellationToken);
        return new MediaBackgroundJobListPage(items, totalItems);
    }
}
