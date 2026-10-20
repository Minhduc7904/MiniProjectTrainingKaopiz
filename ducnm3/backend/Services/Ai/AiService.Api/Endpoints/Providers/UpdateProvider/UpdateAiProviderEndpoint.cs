using AiService.Api.Contracts.Providers;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Providers.Update;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Providers.UpdateProvider;

public static class UpdateAiProviderEndpoint
{
    public static RouteHandlerBuilder MapUpdateAiProvider(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut(ApiRoutes.Ai.ProviderByIdTemplate, async (string providerId, UpdateAiProviderRequest request, HttpContext context, UpdateAiProviderHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new UpdateAiProviderCommand(RouteIdParser.ParseOrEmpty(providerId), request.Name, request.ProviderType, request.BaseUrl, request.ApiKey, request.Status),
                cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier));
        })
        .WithName("update-ai-provider")
        .WithTags(ServiceNames.Ai)
        .Accepts<UpdateAiProviderRequest>("application/json")
        .Produces<ApiResponse<AiProviderResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .RequireActor(ActorAccess.Admin);
}
