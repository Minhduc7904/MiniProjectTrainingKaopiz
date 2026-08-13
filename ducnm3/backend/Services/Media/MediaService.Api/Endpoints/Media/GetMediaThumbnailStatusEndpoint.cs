using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.Features.Derivations;

namespace MediaService.Api.Endpoints.Media;

public static class GetMediaThumbnailStatusEndpoint
{
    public static RouteHandlerBuilder MapGetMediaThumbnailStatus(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapGet(
                ApiRoutes.Media.ThumbnailStatusTemplate,
                async (
                    string mediaId,
                    HttpContext context,
                    GetMediaThumbnailStatusHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        MediaRequestParser.ParseGuid(mediaId, "mediaId"),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Ok(
                        ApiResponseFactory.Success(
                            MediaResponseMapper.ToResponse(result),
                            context.TraceIdentifier));
                })
            .WithName("get-media-thumbnail-status")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<MediaThumbnailStatusResponse>>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
