using AiService.Api.Contracts.Providers;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Providers.HealthCheck;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Providers.HealthCheck;

public static class CheckAiProvidersHealthEndpoint
{
    public static RouteHandlerBuilder MapCheckAiProvidersHealth(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Ai.ProviderHealthChecks, async (HttpContext context, CheckAiProviderHealthHandler handler, CancellationToken cancellationToken) =>
        {
            var results = await handler.HandleAllAsync(cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(results), context.TraceIdentifier));
        })
        .WithName("check-ai-providers-health")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<IReadOnlyList<AiProviderHealthResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .RequireActor(ActorAccess.Admin);
}
