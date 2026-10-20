using AiService.Api.Contracts.Bindings;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Bindings.Upsert;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Bindings.UpsertBinding;

public static class UpsertAiServiceBindingEndpoint
{
    public static RouteHandlerBuilder MapUpsertAiServiceBinding(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut(ApiRoutes.Ai.ServiceBindingByServiceNameTemplate, async (string serviceName, UpsertAiServiceBindingRequest request, HttpContext context, UpsertAiServiceBindingHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new UpsertAiServiceBindingCommand(serviceName, request.ModelId), cancellationToken);
            return Results.Json(
                ApiResponseFactory.Success(AiResponseMapper.ToResponse(result.Binding), context.TraceIdentifier),
                statusCode: result.Created ? StatusCodes.Status201Created : StatusCodes.Status200OK);
        })
        .WithName("upsert-ai-service-binding")
        .WithTags(ServiceNames.Ai)
        .Accepts<UpsertAiServiceBindingRequest>("application/json")
        .Produces<ApiResponse<AiServiceBindingResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<AiServiceBindingResponse>>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .RequireActor(ActorAccess.Admin);
}
