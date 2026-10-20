using AiService.Api.Contracts.Providers;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Providers.GetDetail;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Providers.GetProviderDetail;

public static class GetAiProviderDetailEndpoint
{
    public static RouteHandlerBuilder MapGetAiProviderDetail(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Ai.ProviderByIdTemplate, async (string providerId, HttpContext context, GetAiProviderDetailHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(RouteIdParser.ParseOrEmpty(providerId), cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier));
        })
        .WithName("get-ai-provider-detail")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<AiProviderResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .RequireActor(ActorAccess.Admin);
}
