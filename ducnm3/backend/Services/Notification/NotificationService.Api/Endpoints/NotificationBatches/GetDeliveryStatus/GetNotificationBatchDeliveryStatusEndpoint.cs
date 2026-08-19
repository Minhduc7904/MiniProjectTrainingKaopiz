// File: backend/Services/Notification/NotificationService.Api/Endpoints/NotificationBatches/GetDeliveryStatus/GetNotificationBatchDeliveryStatusEndpoint.cs
// Mục đích: Map GET delivery-status và trả counter gửi chỉ đọc cho UI sau khi snapshot hoàn tất.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.NotificationBatches.Responses;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.NotificationBatches.GetDeliveryStatus;

namespace NotificationService.Api.Endpoints.NotificationBatches.GetDeliveryStatus;

public static class GetNotificationBatchDeliveryStatusEndpoint
{
    public static IEndpointRouteBuilder MapGetNotificationBatchDeliveryStatusEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ApiRoutes.Notifications.BatchDeliveryStatusTemplate, HandleAsync)
            .WithName("get-notification-batch-delivery-status")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationBatchDeliveryStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string batchId,
        HttpContext context,
        GetNotificationBatchDeliveryStatusHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(batchId, out var id) || id == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId must be a valid UUID.");
        }

        var result = await handler.HandleAsync(id, cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(ApiResponseFactory.Success(
            new NotificationBatchDeliveryStatusResponse(
                result.BatchId, result.Status, result.TotalCount, result.ProcessedCount,
                result.SuccessCount, result.FailedCount, result.RemainingCount,
                result.ProgressPercent, result.StartedAtUtc, result.CompletedAtUtc,
                result.DurationMs),
            context.TraceIdentifier));
    }
}
