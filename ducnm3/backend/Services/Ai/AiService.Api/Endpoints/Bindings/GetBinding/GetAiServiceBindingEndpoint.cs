using AiService.Api.Contracts.Bindings;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Bindings.GetDetail;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Bindings.GetBinding;

public static class GetAiServiceBindingEndpoint
{
    public static RouteHandlerBuilder MapGetAiServiceBinding(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Ai.ServiceBindingByServiceNameTemplate, async (string serviceName, HttpContext context, GetAiServiceBindingHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(serviceName, cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier));
        })
        .WithName("get-ai-service-binding")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<AiServiceBindingResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .RequireActor(ActorAccess.Admin);
}
