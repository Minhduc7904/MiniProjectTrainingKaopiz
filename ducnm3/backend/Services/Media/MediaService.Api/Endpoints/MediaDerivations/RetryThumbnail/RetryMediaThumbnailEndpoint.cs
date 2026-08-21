// File: backend/Services/Media/MediaService.Api/Endpoints/MediaDerivations/RetryThumbnail/RetryMediaThumbnailEndpoint.cs
// Mục đích: Khai báo Minimal API endpoint RetryMediaThumbnailEndpoint, chuyển HTTP request thành use case và map response.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.UseCases.MediaDerivations.RetryThumbnail;
using MediaService.Domain.Constants;

namespace MediaService.Api.Endpoints.Media;

public static class RetryMediaThumbnailEndpoint
{
    public static RouteHandlerBuilder MapRetryMediaThumbnail(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapPost(
                ApiRoutes.Media.ThumbnailRetryTemplate,
                async (
                    string mediaId,
                    HttpContext context,
                    RetryMediaThumbnailHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        MediaRequestParser.ParseGuid(mediaId, "mediaId"),
                        MediaRequestParser.ReadActor(context),
                        cancellationToken);
                    context.Response.Headers.Location =
                        ApiRoutes.Media.ThumbnailStatusPublicPath(
                            result.SourceMediaId);
                    return Results.Json(
                        ApiResponseFactory.Success(
                            MediaResponseMapper.ToResponse(result),
                            context.TraceIdentifier),
                        statusCode: StatusCodes.Status202Accepted);
                })
            .WithName("retry-media-thumbnail")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<MediaThumbnailStatusResponse>>(
                StatusCodes.Status202Accepted)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Any);
}
