using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.UseCases.MediaUsages.Create;

namespace MediaService.Api.Endpoints.Media;

public static class CreateMediaUsagesBatchEndpoint
{
    public static RouteHandlerBuilder MapCreateMediaUsagesBatch(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Media.UsageBatch,
                async (
                    CreateMediaUsagesBatchRequest request,
                    HttpContext context,
                    CreateMediaUsageHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    if (request.Items is null || request.Items.Count == 0)
                    {
                        throw new InvalidOperationException("items must contain at least one media usage.");
                    }

                    var results = new List<CreateMediaUsageResponse>(request.Items.Count);
                    foreach (var item in request.Items)
                    {
                        var result = await handler.HandleAsync(
                            new CreateMediaUsageCommand(
                                MediaRequestParser.ParseGuid(item.MediaId, "mediaId"),
                                item.OwnerService,
                                item.OwnerType,
                                MediaRequestParser.ParseGuid(item.OwnerId, "ownerId"),
                                item.UsageType,
                                item.DisplayOrder,
                                MediaRequestParser.ReadActor(context)),
                            cancellationToken);
                        results.Add(MediaResponseMapper.ToResponse(result));
                    }

                    return Results.Json(
                        ApiResponseFactory.Success(results, context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("create-media-usages-batch")
            .WithTags(ServiceNames.Media)
            .Accepts<CreateMediaUsagesBatchRequest>("application/json")
            .Produces<ApiResponse<IReadOnlyList<CreateMediaUsageResponse>>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Any);
}
