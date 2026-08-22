// File: backend/Services/Media/MediaService.Api/Endpoints/Media/GetSummary/GetMediaSummaryEndpoint.cs
// Mục đích: Map endpoint ADMIN đọc tổng số media object cho dashboard.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Application.UseCases.Media.GetSummary;

namespace MediaService.Api.Endpoints.Media.GetSummary;

public static class GetMediaSummaryEndpoint
{
    public static RouteHandlerBuilder MapGetMediaSummary(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Media.Summary,
                async (HttpContext context, GetMediaSummaryHandler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Ok(ApiResponseFactory.Success(result, context.TraceIdentifier));
                })
            .WithName("get-media-summary")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<MediaSummaryResult>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Admin);
}
