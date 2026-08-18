// File: backend/Services/Notification/NotificationService.Api/Endpoints/NotificationBatches/GetById/GetNotificationBatchByIdEndpoint.cs
// Mục đích: Map riêng GET chi tiết Notification Batch và trả trạng thái, counter cùng cache no-store.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.NotificationBatches.Responses;
using NotificationService.Api.Mappers;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.NotificationBatches.GetById;

namespace NotificationService.Api.Endpoints.NotificationBatches.GetById;

public static class GetNotificationBatchByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetNotificationBatchByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ApiRoutes.Notifications.BatchByIdTemplate, HandleAsync)
            .WithName("get-notification-batch-by-id")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationBatchResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string batchId, HttpContext context, GetNotificationBatchByIdHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(batchId, out var id) || id == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId must be a valid UUID.");
        }
        var result = await handler.HandleAsync(id, cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(ApiResponseFactory.Success(
            NotificationResponseMapper.ToResponse(result), context.TraceIdentifier));
    }
}
