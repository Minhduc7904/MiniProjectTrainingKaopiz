using AiService.Api.Contracts.Bindings;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Bindings.GetList;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Bindings.GetBindings;

public static class GetAiServiceBindingsEndpoint
{
    public static RouteHandlerBuilder MapGetAiServiceBindings(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Ai.ServiceBindings, async (HttpContext context, GetAiServiceBindingsHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier));
        })
        .WithName("get-ai-service-bindings")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<IReadOnlyList<AiServiceBindingResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .RequireActor(ActorAccess.Admin);
}
