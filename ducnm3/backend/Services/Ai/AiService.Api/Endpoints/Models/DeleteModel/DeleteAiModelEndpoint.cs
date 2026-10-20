using AiService.Application.UseCases.Models.Delete;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Models.DeleteModel;

public static class DeleteAiModelEndpoint
{
    public static RouteHandlerBuilder MapDeleteAiModel(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete(ApiRoutes.Ai.ModelByIdTemplate, async (string modelId, DeleteAiModelHandler handler, CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(RouteIdParser.ParseOrEmpty(modelId), cancellationToken);
            return Results.NoContent();
        })
        .WithName("delete-ai-model")
        .WithTags(ServiceNames.Ai)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .RequireActor(ActorAccess.Admin);
}
