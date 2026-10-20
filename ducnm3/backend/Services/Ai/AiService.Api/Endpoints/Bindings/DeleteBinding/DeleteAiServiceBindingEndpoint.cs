using AiService.Application.UseCases.Bindings.Delete;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Bindings.DeleteBinding;

public static class DeleteAiServiceBindingEndpoint
{
    public static RouteHandlerBuilder MapDeleteAiServiceBinding(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete(ApiRoutes.Ai.ServiceBindingByServiceNameTemplate, async (string serviceName, DeleteAiServiceBindingHandler handler, CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(serviceName, cancellationToken);
            return Results.NoContent();
        })
        .WithName("delete-ai-service-binding")
        .WithTags(ServiceNames.Ai)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .RequireActor(ActorAccess.Admin);
}
