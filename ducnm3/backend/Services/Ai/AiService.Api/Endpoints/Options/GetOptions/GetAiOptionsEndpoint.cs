using AiService.Api.Contracts.Options;
using AiService.Api.Mappers;
using AiService.Application.UseCases.Options;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;

namespace AiService.Api.Endpoints.Options.GetOptions;

public static class GetAiOptionsEndpoint
{
    public static RouteHandlerBuilder MapGetAiOptions(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Ai.Options, (HttpContext context, GetAiOptionsHandler handler) =>
            Results.Json(ApiResponseFactory.Success(AiResponseMapper.ToResponse(handler.Handle()), context.TraceIdentifier)))
        .WithName("get-ai-options")
        .WithTags(ServiceNames.Ai)
        .Produces<ApiResponse<AiOptionsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .RequireActor(ActorAccess.Admin);
}
