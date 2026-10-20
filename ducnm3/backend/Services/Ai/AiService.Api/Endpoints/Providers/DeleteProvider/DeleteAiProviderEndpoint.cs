using AiService.Application.UseCases.Providers.Delete;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Providers.DeleteProvider;

public static class DeleteAiProviderEndpoint
{
    public static RouteHandlerBuilder MapDeleteAiProvider(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete(ApiRoutes.Ai.ProviderByIdTemplate, async (string providerId, DeleteAiProviderHandler handler, CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(RouteIdParser.ParseOrEmpty(providerId), cancellationToken);
            return Results.NoContent();
        })
        .WithName("delete-ai-provider")
        .WithTags(ServiceNames.Ai)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .RequireActor(ActorAccess.Admin);
}
