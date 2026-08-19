// File: backend/Services/Notification/NotificationService.Api/Endpoints/NotificationBatches/GetSnapshotStatus/GetNotificationBatchSnapshotStatusEndpoint.cs
// Mục đích: Map GET snapshot-status, validate batchId và trả envelope no-store cho UI polling tuần tự.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.NotificationBatches.Responses;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.NotificationBatches.GetSnapshotStatus;

namespace NotificationService.Api.Endpoints.NotificationBatches.GetSnapshotStatus;

public static class GetNotificationBatchSnapshotStatusEndpoint
{
    public static IEndpointRouteBuilder MapGetNotificationBatchSnapshotStatusEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ApiRoutes.Notifications.BatchSnapshotStatusTemplate, HandleAsync)
            .WithName("get-notification-batch-snapshot-status")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationBatchSnapshotStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string batchId,
        HttpContext context,
        GetNotificationBatchSnapshotStatusHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(batchId, out var id) || id == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId must be a valid UUID.");
        }

        var result = await handler.HandleAsync(id, cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(ApiResponseFactory.Success(
            new NotificationBatchSnapshotStatusResponse(
                result.BatchId, result.Status, result.SnapshotCount,
                result.TargetCount, result.ProgressPercent),
            context.TraceIdentifier));
    }
}
