using AiService.Api.Contracts.Providers;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Providers.GetList;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Providers.GetProviders;

public static class GetAiProvidersEndpoint
{
    public static RouteHandlerBuilder MapGetAiProviders(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Ai.Providers, async (string? status, string? providerType, string? search, int? page, int? pageSize, HttpContext context, GetAiProvidersHandler handler, CancellationToken cancellationToken) =>
        {
            var query = GetAiProvidersQuery.Create(status, providerType, search, page, pageSize);
            var result = await handler.HandleAsync(query, cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(
                AiResponseMapper.ToResponse(result.Items),
                context.TraceIdentifier,
                new OffsetPaginationMeta(query.Page, query.PageSize, result.TotalItems, result.TotalPages)));
        })
        .WithName("get-ai-providers")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<IReadOnlyList<AiProviderResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .RequireActor(ActorAccess.Admin);
}
