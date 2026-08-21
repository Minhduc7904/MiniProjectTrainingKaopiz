using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Responses;
using MediaService.Application.UseCases.Media.Library;

namespace MediaService.Api.Endpoints.Media;

public static class GetMediaLibraryEndpoint
{
    public static RouteHandlerBuilder MapGetMediaLibrary(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Media.Library,
                async (
                    string? mediaType,
                    string? cursor,
                    int? pageSize,
                    HttpContext context,
                    GetMediaLibraryHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var actor = MediaRequestParser.ReadActor(context);
                    var result = await handler.HandleAsync(
                        new GetMediaLibraryQuery(
                            mediaType ?? string.Empty,
                            cursor,
                            pageSize ?? 20,
                            actor),
                        cancellationToken);
                    var items = result.Items.Select(item => new MediaLibraryResponse(
                        item.Id,
                        item.MediaType,
                        item.ContentType,
                        item.OriginalFileName,
                        item.SizeBytes,
                        item.Status,
                        item.IsDraft,
                        item.DraftedAtUtc,
                        item.CreatedAtUtc,
                        item.CompletedAtUtc,
                        item.ThumbnailMediaId,
                        ApiRoutes.Media.ContentPublicPath(item.Id),
                        item.ThumbnailStatus ?? "NOT_REQUIRED",
                        item.ThumbnailStatus == "READY" && item.ThumbnailMediaId is { } thumbnailId
                            ? ApiRoutes.Media.ContentPublicPath(thumbnailId)
                            : null)).ToArray();
                    return Results.Json(
                        ApiResponseFactory.Success(
                            new MediaLibraryPageResponse(items, result.NextCursor, result.HasMore),
                            context.TraceIdentifier));
                })
            .WithName("get-media-library")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<MediaLibraryPageResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Any);
}
