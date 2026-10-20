using AiService.Api.Contracts.Models;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Models.Create;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Models.CreateModel;

public static class CreateAiModelEndpoint
{
    public static RouteHandlerBuilder MapCreateAiModel(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Ai.Models, async (CreateAiModelRequest request, HttpContext context, CreateAiModelHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new CreateAiModelCommand(request.ProviderId, request.ModelName, request.DisplayName, request.Status),
                cancellationToken);
            context.Response.Headers.Location = ApiRoutes.Ai.ModelByIdPublicPath(result.Id);
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier), statusCode: StatusCodes.Status201Created);
        })
        .WithName("create-ai-model")
        .WithTags(ServiceNames.Ai)
        .Accepts<CreateAiModelRequest>("application/json")
        .Produces<ApiResponse<AiModelResponse>>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .RequireActor(ActorAccess.Admin);
}
