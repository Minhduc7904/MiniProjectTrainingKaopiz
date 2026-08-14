using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.Features.Usages.GetUrls;

namespace MediaService.Api.Endpoints.Media;

public static class GetMediaUsageUrlEndpoint
{
    public static RouteHandlerBuilder MapGetMediaUsageUrl(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapGet(
                ApiRoutes.Media.UsageUrlTemplate,
                async (
                    string usageId,
                    HttpContext context,
                    GetMediaUsageUrlHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new GetMediaUsageUrlQuery(
                            MediaRequestParser.ParseGuid(usageId, "usageId")),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(
                        ApiResponseFactory.Success(
                            MediaResponseMapper.ToResponse(result),
                            context.TraceIdentifier));
                })
            .WithName("get-media-usage-url")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<MediaUsageUrlResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
