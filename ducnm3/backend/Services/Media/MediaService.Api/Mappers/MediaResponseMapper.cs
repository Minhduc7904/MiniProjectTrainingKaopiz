using BuildingBlocks.Contracts.Api;
using MediaService.Api.Contracts.Responses;
using MediaService.Application.Features.Media.Upload;
using MediaService.Application.Features.Usages.Create;

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
            ApiRoutes.Media.ContentPublicPath(result.Id));

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
}
