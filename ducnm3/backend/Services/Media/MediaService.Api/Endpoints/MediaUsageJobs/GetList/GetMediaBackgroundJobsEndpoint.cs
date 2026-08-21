// File: backend/Services/Media/MediaService.Api/Endpoints/MediaUsageJobs/GetList/GetMediaBackgroundJobsEndpoint.cs
// Mục đích: Map GET danh sách Media background job với filter và offset pagination cho ADMIN.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.MediaUsageJobs;
using MediaService.Api.Endpoints.Media;
using MediaService.Application.UseCases.MediaUsageJobs.GetList;

namespace MediaService.Api.Endpoints.MediaUsageJobs.GetList;

public static class GetMediaBackgroundJobsEndpoint
{
    public static RouteHandlerBuilder MapGetMediaBackgroundJobs(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Media.Jobs,
                async (
                    string? jobType,
                    string? status,
                    string? correlationId,
                    int? page,
                    int? pageSize,
                    HttpContext context,
                    GetMediaBackgroundJobsHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new GetMediaBackgroundJobsQuery(
                            jobType, status, correlationId, page ?? 1, pageSize ?? 20,
                            MediaRequestParser.ReadActor(context)),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    var items = result.Items.Select(ToResponse).ToArray();
                    return Results.Ok(ApiResponseFactory.Success(
                        items,
                        context.TraceIdentifier,
                        new OffsetPaginationMeta(
                            result.Page, result.PageSize, result.TotalItems, result.TotalPages)));
                })
            .WithName("get-media-background-jobs")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<IReadOnlyList<MediaBackgroundJobListResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Admin);

    private static MediaBackgroundJobListResponse ToResponse(
        MediaService.Application.Repositories.MediaBackgroundJobListRecord item)
    {
        var handled = checked(item.ProcessedItemCount + item.FailedItemCount);
        uint? remaining = item.ExpectedItemCount is { } expected && expected > handled
            ? expected - handled
            : item.ExpectedItemCount is not null ? 0 : null;
        decimal? progress = item.ExpectedItemCount is > 0
            ? Math.Min(100m, Math.Round(handled * 100m / item.ExpectedItemCount.Value, 2))
            : item.ExpectedItemCount == 0 ? 100m : null;
        return new MediaBackgroundJobListResponse(
            item.Id, item.JobType, item.SubjectType, item.SubjectId, item.CorrelationId,
            item.Status, item.ExpectedItemCount, item.ProcessedItemCount,
            item.FailedItemCount, remaining, progress, item.AttemptCount, item.LastError,
            item.CreatedAtUtc, item.StartedAtUtc, item.CompletedAtUtc, item.UpdatedAtUtc);
    }
}
