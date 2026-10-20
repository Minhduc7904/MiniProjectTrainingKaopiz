using AiService.Api.Contracts.Models;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Models.GetDetail;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Models.GetModelDetail;

public static class GetAiModelDetailEndpoint
{
    public static RouteHandlerBuilder MapGetAiModelDetail(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Ai.ModelByIdTemplate, async (string modelId, HttpContext context, GetAiModelDetailHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(RouteIdParser.ParseOrEmpty(modelId), cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier));
        })
        .WithName("get-ai-model-detail")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<AiModelResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .RequireActor(ActorAccess.Admin);
}
