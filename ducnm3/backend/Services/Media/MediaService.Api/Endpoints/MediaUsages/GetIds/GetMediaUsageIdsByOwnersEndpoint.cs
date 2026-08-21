using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.MediaUsages.GetIds;
using MediaService.Application.UseCases.MediaUsages.GetIds;

namespace MediaService.Api.Endpoints.Media;

public static class GetMediaUsageIdsByOwnersEndpoint
{
    public static RouteHandlerBuilder MapGetMediaUsageIdsByOwners(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Media.UsageIdsQuery,
                async (
                    GetMediaUsageIdsByOwnersRequest request,
                    HttpContext context,
                    GetMediaUsageIdsByOwnersHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    var owners = (request.Owners ?? [])
                        .Select(owner => new MediaUsageOwnerScopeQuery(
                            owner.OwnerService ?? string.Empty,
                            owner.OwnerType ?? string.Empty,
                            MediaRequestParser.ParseGuid(owner.OwnerId ?? string.Empty, "ownerId")))
                        .ToArray();
                    var usageIds = await handler.HandleAsync(
                        new GetMediaUsageIdsByOwnersQuery(owners),
                        cancellationToken);
                    return Results.Json(ApiResponseFactory.Success(usageIds, context.TraceIdentifier));
                })
            .WithName("get-media-usage-ids-by-owners")
            .WithTags(ServiceNames.Media)
            .Accepts<GetMediaUsageIdsByOwnersRequest>("application/json")
            .Produces<ApiResponse<IReadOnlyList<Guid>>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
}
