using AiService.Api.Contracts.Models;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Models.GetList;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Models.GetModels;

public static class GetAiModelsEndpoint
{
    public static RouteHandlerBuilder MapGetAiModels(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Ai.Models, async (Guid? providerId, string? status, string? search, int? page, int? pageSize, HttpContext context, GetAiModelsHandler handler, CancellationToken cancellationToken) =>
        {
            var query = GetAiModelsQuery.Create(providerId, status, search, page, pageSize);
            var result = await handler.HandleAsync(query, cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(
                AiResponseMapper.ToResponse(result.Items),
                context.TraceIdentifier,
                new OffsetPaginationMeta(query.Page, query.PageSize, result.TotalItems, result.TotalPages)));
        })
        .WithName("get-ai-models")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<IReadOnlyList<AiModelResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .RequireActor(ActorAccess.Admin);
}
