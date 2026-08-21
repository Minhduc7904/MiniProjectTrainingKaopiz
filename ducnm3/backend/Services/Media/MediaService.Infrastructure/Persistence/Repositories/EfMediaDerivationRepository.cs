// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Repositories/EfMediaDerivationRepository.cs
// Mục đích: Thực thi thumbnail derivation bằng media_background_jobs thay cho bảng job chuyên biệt.

using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Application.Contracts.Messaging;
using MediaService.Application.Repositories;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence.Repositories;

public sealed class EfMediaDerivationRepository(
    MediaDbContext dbContext,
    EfMediaBackgroundJobRepository backgroundJobs,
    ICommandSender commandSender,
    IMediaUsageRepository mediaUsageRepository,
    TimeProvider timeProvider) : IMediaDerivationRepository
{
    public async Task<ThumbnailDerivationWork?> BeginAsync(Guid jobId, Guid sourceMediaId, Guid derivativeMediaId, CancellationToken cancellationToken)
    {
        var job = await backgroundJobs.GetThumbnailTrackedAsync(jobId, sourceMediaId, cancellationToken);
        var payload = job is null ? null : ReadPayload(job.PayloadJson);
        var source = await dbContext.MediaObjects.SingleOrDefaultAsync(item => item.Id == sourceMediaId, cancellationToken);
        var derivative = await dbContext.MediaObjects.SingleOrDefaultAsync(item => item.Id == derivativeMediaId, cancellationToken);
        if (job is null || payload?.DerivativeMediaId != derivativeMediaId || source is null || derivative is null || source.DeletedAt is not null || source.Status != MediaObjectStatuses.Ready) return null;

        if (job.Status != MediaBackgroundJobStatuses.Completed)
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            job.Status = MediaBackgroundJobStatuses.Processing;
            job.AttemptCount++;
            job.StartedAt ??= now;
            job.CompletedAt = null;
            job.LastError = null;
            job.UpdatedAt = now;
            derivative.Status = MediaObjectStatuses.Pending;
            derivative.FailureReason = null;
            derivative.UpdatedAt = now;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new ThumbnailDerivationWork(job.Id, sourceMediaId, new(source.Bucket, source.ObjectKey), source.MediaType, source.ContentType, source.Status, derivativeMediaId, new(derivative.Bucket, derivative.ObjectKey), derivative.Status, ToThumbnailStatus(job.Status), new(source.UploadedByType, source.UploadedBy));
    }

    public async Task CompleteAsync(Guid jobId, Guid derivativeMediaId, string checksumSha256, long sizeBytes, DateTime completedAtUtc, CancellationToken cancellationToken)
    {
        var job = await backgroundJobs.GetThumbnailByIdAsync(jobId, cancellationToken) ?? throw new InvalidOperationException("Thumbnail job was not found.");
        var payload = ReadPayload(job.PayloadJson) ?? throw new InvalidOperationException("Thumbnail job payload is invalid.");
        if (payload.DerivativeMediaId != derivativeMediaId || payload.SourceMediaId is not { } sourceMediaId) throw new InvalidOperationException("Thumbnail job payload does not match command.");
        var derivative = await dbContext.MediaObjects.SingleAsync(item => item.Id == derivativeMediaId, cancellationToken);
        var source = await dbContext.MediaObjects.SingleAsync(item => item.Id == sourceMediaId, cancellationToken);
        if (job.Status == MediaBackgroundJobStatuses.Completed && derivative.Status == MediaObjectStatuses.Ready) return;

        derivative.Status = MediaObjectStatuses.Ready;
        derivative.ChecksumSha256 = checksumSha256;
        derivative.SizeBytes = checked((ulong)sizeBytes);
        derivative.CompletedAt = completedAtUtc;
        derivative.UpdatedAt = completedAtUtc;
        derivative.FailureReason = null;
        job.Status = MediaBackgroundJobStatuses.Completed;
        job.CompletedAt = completedAtUtc;
        job.UpdatedAt = completedAtUtc;
        job.LastError = null;
        job.ProcessedItemCount = 1;
        await mediaUsageRepository.EnsureThumbnailDerivationUsageAsync(new(Guid.NewGuid(), derivativeMediaId, MediaOwnerServices.Media, MediaOwnerTypes.MediaThumbnail, sourceMediaId, MediaUsageTypes.Thumbnail, 0, new ActorReference(source.UploadedByType, source.UploadedBy)), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(Guid jobId, Guid derivativeMediaId, string safeError, DateTime failedAtUtc, CancellationToken cancellationToken)
    {
        var job = await backgroundJobs.GetThumbnailByIdAsync(jobId, cancellationToken);
        if (job is null || job.Status == MediaBackgroundJobStatuses.Completed) return;
        var payload = ReadPayload(job.PayloadJson);
        if (payload?.DerivativeMediaId != derivativeMediaId) return;
        var derivative = await dbContext.MediaObjects.SingleOrDefaultAsync(item => item.Id == derivativeMediaId, cancellationToken);
        job.Status = MediaBackgroundJobStatuses.Failed;
        job.FailedItemCount = 1;
        job.LastError = Truncate(safeError);
        job.CompletedAt = failedAtUtc;
        job.UpdatedAt = failedAtUtc;
        if (derivative is not null) { derivative.Status = MediaObjectStatuses.Failed; derivative.FailureReason = Truncate(safeError); derivative.UpdatedAt = failedAtUtc; }
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MediaThumbnailStatusRecord?> GetThumbnailStatusAsync(Guid sourceMediaId, CancellationToken cancellationToken)
    {
        var job = await backgroundJobs.GetThumbnailBySourceAsync(sourceMediaId, false, cancellationToken);
        var payload = job is null ? null : ReadPayload(job.PayloadJson);
        if (job is null || payload?.DerivativeMediaId is not { } derivativeMediaId) return null;
        var active = await dbContext.MediaUsages.AsNoTracking().Where(usage => usage.OwnerService == MediaOwnerServices.Media && usage.OwnerType == MediaOwnerTypes.MediaThumbnail && usage.OwnerId == sourceMediaId && usage.UsageType == MediaUsageTypes.Thumbnail && usage.DeletedAt == null).Select(usage => (Guid?)usage.MediaId).SingleOrDefaultAsync(cancellationToken);
        return new(sourceMediaId, job.Id, ToThumbnailStatus(job.Status), derivativeMediaId, active, job.LastError, job.UpdatedAt);
    }

    public async Task<MediaThumbnailStatusRecord> RetryAsync(Guid sourceMediaId, ActorReference requestedBy, CancellationToken cancellationToken)
    {
        var job = await backgroundJobs.GetThumbnailBySourceAsync(sourceMediaId, true, cancellationToken) ?? throw MediaErrors.ThumbnailNotFound();
        var payload = ReadPayload(job.PayloadJson) ?? throw MediaErrors.ThumbnailNotFound();
        var source = await dbContext.MediaObjects.SingleAsync(item => item.Id == sourceMediaId, cancellationToken);
        if (source.UploadedBy != requestedBy.Id || !string.Equals(source.UploadedByType, requestedBy.Type, StringComparison.Ordinal)) throw MediaErrors.ThumbnailNotFound();
        if (job.Status != MediaBackgroundJobStatuses.Failed || payload.DerivativeMediaId is not { } derivativeMediaId) throw MediaErrors.ThumbnailRetryConflict();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        job.Status = MediaBackgroundJobStatuses.Queued;
        job.FailedItemCount = 0;
        job.LastError = null;
        job.CompletedAt = null;
        job.UpdatedAt = now;
        await commandSender.SendAsync(ServiceNames.Media, new GenerateMediaThumbnailV1(job.Id, sourceMediaId, derivativeMediaId), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new(sourceMediaId, job.Id, MediaDerivationStatuses.Queued, derivativeMediaId, null, null, now);
    }

    private static MediaBackgroundJobPayload? ReadPayload(string value) => JsonSerializer.Deserialize<MediaBackgroundJobPayload>(value);
    private static string ToThumbnailStatus(string status) => status == MediaBackgroundJobStatuses.Completed ? MediaDerivationStatuses.Ready : status;
    private static string Truncate(string value) => value.Length <= 500 ? value : value[..500];
}
