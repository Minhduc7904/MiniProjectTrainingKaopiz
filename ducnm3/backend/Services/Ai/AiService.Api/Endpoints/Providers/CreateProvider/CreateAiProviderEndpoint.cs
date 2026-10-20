using AiService.Api.Contracts.Providers;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Providers.Create;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Providers.CreateProvider;

public static class CreateAiProviderEndpoint
{
    public static RouteHandlerBuilder MapCreateAiProvider(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Ai.Providers, async (CreateAiProviderRequest request, HttpContext context, CreateAiProviderHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new CreateAiProviderCommand(request.Name, request.ProviderType, request.BaseUrl, request.ApiKey, request.Status),
                cancellationToken);
            context.Response.Headers.Location = ApiRoutes.Ai.ProviderByIdPublicPath(result.Id);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(result), context.TraceIdentifier), statusCode: StatusCodes.Status201Created);
        })
        .WithName("create-ai-provider")
        .WithTags(ServiceNames.Ai)
        .Accepts<CreateAiProviderRequest>("application/json")
        .Produces<ApiResponse<AiProviderResponse>>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .RequireActor(ActorAccess.Admin);
}
