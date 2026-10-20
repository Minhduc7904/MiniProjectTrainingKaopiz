using AiService.Api.Contracts.Providers;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Providers.HealthCheck;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Providers.HealthCheck;

public static class CheckAiProviderHealthEndpoint
{
    public static RouteHandlerBuilder MapCheckAiProviderHealth(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Ai.ProviderHealthCheckByIdTemplate, async (string providerId, HttpContext context, CheckAiProviderHealthHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(RouteIdParser.ParseOrEmpty(providerId), cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier));
        })
        .WithName("check-ai-provider-health")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<AiProviderHealthResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .RequireActor(ActorAccess.Admin);
}
