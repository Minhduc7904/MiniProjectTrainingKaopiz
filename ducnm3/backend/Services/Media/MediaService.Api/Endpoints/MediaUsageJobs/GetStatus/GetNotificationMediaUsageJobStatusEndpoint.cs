// File: backend/Services/Media/MediaService.Api/Endpoints/MediaUsageJobs/GetStatus/GetNotificationMediaUsageJobStatusEndpoint.cs
// Mục đích: Map GET status job Media Usage tại đúng Media Service và đặt cache no-store cho polling.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.MediaUsageJobs;
using MediaService.Api.Endpoints.Media;
using MediaService.Application.UseCases.MediaUsageJobs.GetStatus;

namespace MediaService.Api.Endpoints.MediaUsageJobs.GetStatus;

public static class GetNotificationMediaUsageJobStatusEndpoint
{
    public static RouteHandlerBuilder MapGetNotificationMediaUsageJobStatus(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Media.NotificationMediaUsageJobStatusTemplate,
                async (
                    string jobId,
                    HttpContext context,
                    GetNotificationMediaUsageJobStatusHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        MediaRequestParser.ParseGuid(jobId, "jobId"),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Ok(ApiResponseFactory.Success(
                        new NotificationMediaUsageJobStatusResponse(
                            result.JobId, result.Status, result.ExpectedUsageCount,
                            result.ProcessedUsageCount, result.FailedUsageCount,
                            result.RemainingUsageCount, result.ProgressPercent,
                            result.CreatedAtUtc, result.StartedAtUtc,
                            result.CompletedAtUtc, result.ErrorMessage),
                        context.TraceIdentifier));
                })
            .WithName("get-notification-media-usage-job-status")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<NotificationMediaUsageJobStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
