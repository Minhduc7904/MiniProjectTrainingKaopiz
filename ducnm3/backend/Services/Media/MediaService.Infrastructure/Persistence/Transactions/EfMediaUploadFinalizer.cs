// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Transactions/EfMediaUploadFinalizer.cs
// Mục đích: Dùng transaction EF để claim upload draft, cập nhật Media sang ready, tạo thumbnail job/outbox và trả kết quả cạnh tranh an toàn.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Application.Contracts.Messaging;
using MediaService.Application.Repositories;
using MediaService.Domain.Constants;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Repositories;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MediaService.Infrastructure.Persistence.Transactions;

public sealed class EfMediaUploadFinalizer(
    MediaDbContext dbContext,
    EfMediaBackgroundJobRepository backgroundJobs,
    ICommandSender commandSender) : IMediaUploadFinalizer
{
    public async Task<MediaUploadFinalizationResult> FinalizeAsync(
        MediaUploadFinalizationRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken);

        var source = await dbContext.MediaObjects
            .FromSqlInterpolated(
                $"SELECT * FROM media_objects WHERE id = {request.SourceMediaId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken) ?? throw MediaErrors.MediaNotFound();
        if (request.RequestedBy is { } requestedBy &&
            (source.UploadedBy != requestedBy.Id ||
             !string.Equals(source.UploadedByType, requestedBy.Type, StringComparison.Ordinal)))
        {
            throw MediaErrors.MediaNotFound();
        }

        if (source.Status == MediaObjectStatuses.Ready)
        {
            var establishedJob = await backgroundJobs.Query()
                .AsNoTracking()
                .Where(job => job.JobType == MediaBackgroundJobTypes.ThumbnailDerivation &&
                    job.SubjectId == request.SourceMediaId)
                .Select(job => new
                {
                    job.Id,
                    job.Status,
                    job.PayloadJson,
                })
                .SingleOrDefaultAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new MediaUploadFinalizationResult(
                source.CompletedAt ?? request.CompletedAtUtc,
                ToThumbnailStatus(establishedJob?.Status),
                establishedJob is null ? null : ReadDerivativeMediaId(establishedJob.PayloadJson),
                establishedJob?.Id,
                false);
        }

        if (source.Status != MediaObjectStatuses.Pending ||
            (request.RequestedBy is not null &&
             !string.Equals(source.ChecksumSha256, request.SourceChecksumSha256, StringComparison.Ordinal)))
        {
            throw MediaErrors.DirectUploadIncomplete();
        }
        source.Status = MediaObjectStatuses.Ready;
        source.ChecksumSha256 = request.SourceChecksumSha256;
        source.CompletedAt = request.CompletedAtUtc;
        source.IsDraft = true;
        source.DraftedAt = request.CompletedAtUtc;
        source.UpdatedAt = request.CompletedAtUtc;
        source.FailureReason = null;
        if (request.FinalLocation is not null)
        {
            source.Bucket = request.FinalLocation.Bucket;
            source.ObjectKey = request.FinalLocation.ObjectKey;
        }

        if (request.Thumbnail is null)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new MediaUploadFinalizationResult(
                request.CompletedAtUtc,
                MediaDerivationStatuses.NotRequired,
                null,
                null);
        }

        var thumbnail = request.Thumbnail;
        dbContext.MediaObjects.Add(
            new MediaObject
            {
                Id = thumbnail.MediaId,
                SourceMediaId = request.SourceMediaId,
                DerivationType = MediaDerivationTypes.Thumbnail,
                Bucket = thumbnail.Location.Bucket,
                ObjectKey = thumbnail.Location.ObjectKey,
                MediaType = MediaTypes.Image,
                ContentType = "image/webp",
                OriginalFileName = thumbnail.OriginalFileName,
                SizeBytes = 0,
                ChecksumSha256 = null,
                UploadedBy = thumbnail.UploadedBy.Id,
                UploadedByType = thumbnail.UploadedBy.Type,
                Status = MediaObjectStatuses.Pending,
                IsDraft = true,
                DraftedAt = null,
                CreatedAt = request.CompletedAtUtc,
                UpdatedAt = request.CompletedAtUtc,
            });
        backgroundJobs.Add(
            new MediaBackgroundJob
            {
                Id = thumbnail.JobId,
                JobType = MediaBackgroundJobTypes.ThumbnailDerivation,
                SubjectType = "MEDIA",
                SubjectId = request.SourceMediaId,
                DeduplicationKey = $"THUMBNAIL:{request.SourceMediaId:D}",
                Status = MediaBackgroundJobStatuses.Queued,
                PayloadJson = JsonSerializer.Serialize(new MediaBackgroundJobPayload(
                    1, request.SourceMediaId, thumbnail.MediaId)),
                AttemptCount = 0,
                CreatedAt = request.CompletedAtUtc,
                UpdatedAt = request.CompletedAtUtc,
            });

        await commandSender.SendAsync(
            ServiceNames.Media,
            new GenerateMediaThumbnailV1(
                thumbnail.JobId,
                request.SourceMediaId,
                thumbnail.MediaId),
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new MediaUploadFinalizationResult(
            request.CompletedAtUtc,
            MediaDerivationStatuses.Queued,
            thumbnail.MediaId,
            thumbnail.JobId);
    }

    private static Guid? ReadDerivativeMediaId(string payloadJson) =>
        JsonSerializer.Deserialize<MediaBackgroundJobPayload>(payloadJson)?.DerivativeMediaId;

    private static string ToThumbnailStatus(string? status) => status switch
    {
        MediaBackgroundJobStatuses.Completed => MediaDerivationStatuses.Ready,
        null => MediaDerivationStatuses.NotRequired,
        _ => status,
    };
}
