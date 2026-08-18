// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Transactions/EfMediaUploadFinalizer.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Application;
using MediaService.Application.Repositories;
using MediaService.Application.Contracts.Messaging;
using MediaService.Domain.Constants;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

using MediaService.Application.Common.Errors;

namespace MediaService.Infrastructure.Persistence.Transactions;

public sealed class EfMediaUploadFinalizer(
    MediaDbContext dbContext,
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
            var establishedJob = await dbContext.MediaDerivationJobs
                .AsNoTracking()
                .Where(job => job.SourceMediaId == request.SourceMediaId)
                .Select(job => new
                {
                    job.Id,
                    job.DerivativeMediaId,
                    job.Status,
                })
                .SingleOrDefaultAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new MediaUploadFinalizationResult(
                source.CompletedAt ?? request.CompletedAtUtc,
                establishedJob?.Status ?? MediaDerivationStatuses.NotRequired,
                establishedJob?.DerivativeMediaId,
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
        dbContext.MediaDerivationJobs.Add(
            new MediaDerivationJob
            {
                Id = thumbnail.JobId,
                SourceMediaId = request.SourceMediaId,
                DerivativeMediaId = thumbnail.MediaId,
                DerivationType = MediaDerivationTypes.Thumbnail,
                Status = MediaDerivationStatuses.Queued,
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
}
