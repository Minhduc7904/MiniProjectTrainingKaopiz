using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Requests;
using MediaService.Application.Repositories;
using MediaService.Domain.Constants;

namespace MediaService.Api.Endpoints.Media;

public static class ManageMediaUsagesEndpoints
{
    public static void MapManageMediaUsages(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(
                $"{ApiRoutes.Media.Usages}/{{usageId}}",
                async (
                    string usageId,
                    HttpContext context,
                    IMediaUsageRepository repository,
                    CancellationToken cancellationToken) =>
                {
                    var actor = MediaRequestParser.ReadActor(context);
                    await repository.RemoveAsync(
                        MediaRequestParser.ParseGuid(usageId, "usageId"),
                        actor,
                        cancellationToken);
                    return Results.NoContent();
                })
            .WithName("remove-media-usage")
            .WithTags(ServiceNames.Media)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Any);

        endpoints.MapPost(
                ApiRoutes.Media.UsageReorder,
                async (
                    ReorderMediaUsagesRequest request,
                    HttpContext context,
                    IMediaUsageRepository repository,
                    CancellationToken cancellationToken) =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    var usageIds = request.UsageIds
                        .Select(value => MediaRequestParser.ParseGuid(value, "usageIds"))
                        .ToArray();
                    await repository.ReorderAsync(
                        request.OwnerService.Trim().ToUpperInvariant(),
                        request.OwnerType.Trim().ToUpperInvariant(),
                        MediaRequestParser.ParseGuid(request.OwnerId, "ownerId"),
                        usageIds,
                        MediaRequestParser.ReadActor(context),
                        cancellationToken);
                    return Results.Json(
                        ApiResponseFactory.Success(
                            new { updated = usageIds.Length },
                            context.TraceIdentifier));
                })
            .WithName("reorder-media-usages")
            .WithTags(ServiceNames.Media)
            .Accepts<ReorderMediaUsagesRequest>("application/json")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Any);
    }
}
