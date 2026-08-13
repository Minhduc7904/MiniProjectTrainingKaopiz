using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.Features.Derivations;
using MediaService.Domain.Actors;

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
                    RetryMediaThumbnailRequest request,
                    HttpContext context,
                    RetryMediaThumbnailHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    var result = await handler.HandleAsync(
                        MediaRequestParser.ParseGuid(mediaId, "mediaId"),
                        new ActorReference(
                            request.RequestedByType,
                            MediaRequestParser.ParseGuid(
                                request.RequestedBy,
                                "requestedBy")),
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
            .Accepts<RetryMediaThumbnailRequest>("application/json")
            .Produces<ApiResponse<MediaThumbnailStatusResponse>>(
                StatusCodes.Status202Accepted)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
}
