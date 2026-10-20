using AiService.Api.Contracts.Models;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Models.Update;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Models.UpdateModel;

public static class UpdateAiModelEndpoint
{
    public static RouteHandlerBuilder MapUpdateAiModel(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut(ApiRoutes.Ai.ModelByIdTemplate, async (string modelId, UpdateAiModelRequest request, HttpContext context, UpdateAiModelHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new UpdateAiModelCommand(RouteIdParser.ParseOrEmpty(modelId), request.ModelName, request.DisplayName, request.Status),
                cancellationToken);
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier));
        })
        .WithName("update-ai-model")
        .WithTags(ServiceNames.Ai)
        .Accepts<UpdateAiModelRequest>("application/json")
        .Produces<ApiResponse<AiModelResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .RequireActor(ActorAccess.Admin);
}
