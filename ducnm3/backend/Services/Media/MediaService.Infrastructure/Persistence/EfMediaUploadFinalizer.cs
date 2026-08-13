using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Contracts.Messaging;
using MediaService.Domain.Media;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence;

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

        var source = await dbContext.MediaObjects.SingleAsync(
            item => item.Id == request.SourceMediaId,
            cancellationToken);
        source.Status = MediaObjectStatuses.Ready;
        source.ChecksumSha256 = request.SourceChecksumSha256;
        source.CompletedAt = request.CompletedAtUtc;
        source.UpdatedAt = request.CompletedAtUtc;
        source.FailureReason = null;

        if (request.Thumbnail is null)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new MediaUploadFinalizationResult(
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
            MediaDerivationStatuses.Queued,
            thumbnail.MediaId,
            thumbnail.JobId);
    }
}
