using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Responses;
using MediaService.Application.Repositories;
using MediaService.Application.UseCases.Media.Library;
using MediaService.Domain.Constants;

namespace MediaService.Api.Endpoints.Media;

public static class GetMediaLibraryEndpoint
{
    public static RouteHandlerBuilder MapGetMediaLibrary(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Media.Library,
                async (
                    string? mediaType,
                    string? status,
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
                            status ?? string.Empty,
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
                        item.Thumbnail?.Id,
                        ApiRoutes.Media.ContentPublicPath(item.Id),
                        GetThumbnailStatus(item.Thumbnail),
                        GetThumbnailContentUrl(item.Thumbnail),
                        ToThumbnailResponse(item.Thumbnail))).ToArray();
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

    private static MediaLibraryThumbnailResponse? ToThumbnailResponse(
        MediaLibraryThumbnailRecord? thumbnail)
    {
        if (thumbnail is null)
        {
            return null;
        }

        var status = GetThumbnailStatus(thumbnail);
        return new MediaLibraryThumbnailResponse(
            thumbnail.Id,
            status,
            thumbnail.ContentType,
            thumbnail.SizeBytes,
            thumbnail.CreatedAtUtc,
            thumbnail.CompletedAtUtc,
            status == MediaDerivationStatuses.Ready
                ? ApiRoutes.Media.ContentPublicPath(thumbnail.Id)
                : null);
    }

    private static string? GetThumbnailContentUrl(
        MediaLibraryThumbnailRecord? thumbnail) =>
        GetThumbnailStatus(thumbnail) == MediaDerivationStatuses.Ready && thumbnail is not null
            ? ApiRoutes.Media.ContentPublicPath(thumbnail.Id)
            : null;

    private static string GetThumbnailStatus(MediaLibraryThumbnailRecord? thumbnail)
    {
        if (thumbnail is null)
        {
            return MediaDerivationStatuses.NotRequired;
        }

        return thumbnail.BackgroundJobStatus switch
        {
            MediaBackgroundJobStatuses.Completed when thumbnail.MediaStatus == MediaObjectStatuses.Ready =>
                MediaDerivationStatuses.Ready,
            MediaBackgroundJobStatuses.Queued => MediaDerivationStatuses.Queued,
            MediaBackgroundJobStatuses.Processing => MediaDerivationStatuses.Processing,
            MediaBackgroundJobStatuses.Failed or MediaBackgroundJobStatuses.PartialFailed =>
                MediaDerivationStatuses.Failed,
            _ when thumbnail.MediaStatus == MediaObjectStatuses.Ready =>
                MediaDerivationStatuses.Ready,
            _ when thumbnail.MediaStatus == MediaObjectStatuses.Failed =>
                MediaDerivationStatuses.Failed,
            _ => MediaDerivationStatuses.Queued,
        };
    }
}
