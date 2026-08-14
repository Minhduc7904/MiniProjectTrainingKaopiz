using BuildingBlocks.Contracts.Api;
using MediaService.Api.Contracts.Responses;
using MediaService.Application.Features.Media.Upload;
using MediaService.Application.Features.Derivations;
using MediaService.Application.Features.Usages.Create;
using MediaService.Application.Features.Usages.GetUrls;

namespace MediaService.Api.Mappers;

public static class MediaResponseMapper
{
    public static UploadMediaResponse ToResponse(UploadMediaResult result) =>
        new(
            result.Id,
            result.MediaType,
            result.ContentType,
            result.SizeBytes,
            result.Status,
            ApiRoutes.Media.ContentPublicPath(result.Id),
            result.ThumbnailStatus,
            result.ThumbnailMediaId,
            result.ThumbnailJobId,
            result.ThumbnailJobId.HasValue
                ? ApiRoutes.Media.ThumbnailStatusPublicPath(result.Id)
                : null);

    public static MediaThumbnailStatusResponse ToResponse(
        MediaThumbnailStatusResult result) =>
        new(
            result.SourceMediaId,
            result.JobId,
            result.Status,
            result.ThumbnailMediaId,
            result.ActiveThumbnailMediaId,
            result.ActiveThumbnailMediaId is { } mediaId
                ? ApiRoutes.Media.ContentPublicPath(mediaId)
                : null,
            result.LastError,
            result.UpdatedAtUtc);

    public static CreateMediaUsageResponse ToResponse(
        CreateMediaUsageResult result) =>
        new(
            result.Id,
            result.MediaId,
            result.OwnerService,
            result.OwnerType,
            result.OwnerId,
            result.UsageType,
            result.DisplayOrder,
            result.CreatedAtUtc);

    public static MediaUsageUrlResponse ToResponse(
        MediaUsageUrlResult result) =>
        new(
            result.UsageId,
            result.MediaId,
            result.Url,
            result.ExpiresAtUtc,
            result.DisplayOrder);
}
