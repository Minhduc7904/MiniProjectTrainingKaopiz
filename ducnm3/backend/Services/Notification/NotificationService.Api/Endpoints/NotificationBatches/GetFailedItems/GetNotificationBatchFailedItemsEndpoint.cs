// File: backend/Services/Notification/NotificationService.Api/Endpoints/NotificationBatches/GetFailedItems/GetNotificationBatchFailedItemsEndpoint.cs
// Mục đích: Map riêng GET danh sách item gửi lỗi của Notification Batch theo cursor và giới hạn trang hiện có.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.NotificationBatches.Responses;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.NotificationBatches.GetFailedItems;

namespace NotificationService.Api.Endpoints.NotificationBatches.GetFailedItems;

public static class GetNotificationBatchFailedItemsEndpoint
{
    public static IEndpointRouteBuilder MapGetNotificationBatchFailedItemsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ApiRoutes.Notifications.BatchFailedItemsTemplate, HandleAsync)
            .WithName("get-notification-batch-failed-items")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationBatchFailedItemsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string batchId, string? cursor, int? limit, HttpContext context,
        GetNotificationBatchFailedItemsHandler handler, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(batchId, out var id) || id == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId must be a valid UUID.");
        }
        var pageLimit = limit ?? 100;
        var page = await handler.HandleAsync(id, cursor, pageLimit, cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(ApiResponseFactory.Success(
            new NotificationBatchFailedItemsResponse(page.Items.Select(item =>
                new NotificationBatchFailedItemResponse(item.StudentId, item.RetryCount, item.ErrorMessage)).ToArray()),
            context.TraceIdentifier,
            new CursorPaginationMeta(pageLimit,
                GetNotificationBatchFailedItemsHandler.EncodeCursor(page.NextItemId), page.HasNextPage)));
    }
}
