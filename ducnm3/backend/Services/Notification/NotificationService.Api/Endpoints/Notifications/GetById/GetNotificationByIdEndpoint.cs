// File: backend/Services/Notification/NotificationService.Api/Endpoints/Notifications/GetById/GetNotificationByIdEndpoint.cs
// Mục đích: Map riêng GET chi tiết Notification theo ID và trả representation hiện có với cache no-store.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.Notifications.Responses;
using NotificationService.Api.Mappers;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.Notifications.GetById;

namespace NotificationService.Api.Endpoints.Notifications.GetById;

public static class GetNotificationByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetNotificationByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ApiRoutes.Notifications.ItemByIdTemplate, HandleAsync)
            .WithName("get-notification-by-id")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string notificationId, HttpContext context, GetNotificationByIdHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(notificationId, out var id) || id == Guid.Empty)
        {
            throw NotificationErrors.Validation("notificationId must be a valid UUID.");
        }
        var result = await handler.HandleAsync(id, cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(ApiResponseFactory.Success(
            NotificationResponseMapper.ToResponse(result), context.TraceIdentifier));
    }
}
