using AiService.Api.Contracts.Completions;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Completions.Create;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Completions.CreateCompletion;

public static class CreateAiCompletionEndpoint
{
    public static RouteHandlerBuilder MapCreateAiCompletion(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Ai.Completions, async (CreateAiCompletionRequest request, HttpContext context, CreateAiCompletionHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(AiRequestMapper.ToCommand(request), cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier));
        })
        .WithName("create-ai-completion")
        .WithTags(ServiceNames.Ai)
        .Accepts<CreateAiCompletionRequest>("application/json")
        .Produces<ApiResponse<AiCompletionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .Produces<ApiErrorResponse>(StatusCodes.Status429TooManyRequests)
        .Produces<ApiErrorResponse>(StatusCodes.Status502BadGateway)
        .Produces<ApiErrorResponse>(StatusCodes.Status504GatewayTimeout)
        .RequireActor(ActorAccess.Admin);
}
