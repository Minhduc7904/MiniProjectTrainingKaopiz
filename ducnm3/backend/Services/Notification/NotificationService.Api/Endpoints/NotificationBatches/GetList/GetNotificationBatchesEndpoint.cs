// File: backend/Services/Notification/NotificationService.Api/Endpoints/NotificationBatches/GetList/GetNotificationBatchesEndpoint.cs
// Mục đích: Map riêng GET collection Notification Batch với status filter, offset pagination và no-store cho trang quản lý.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.NotificationBatches.Responses;
using NotificationService.Api.Mappers;
using NotificationService.Application.UseCases.NotificationBatches.GetList;

namespace NotificationService.Api.Endpoints.NotificationBatches.GetList;

public static class GetNotificationBatchesEndpoint
{
    public static IEndpointRouteBuilder MapGetNotificationBatchesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ApiRoutes.Notifications.Batches, HandleAsync)
            .WithName("get-notification-batches")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<IReadOnlyList<NotificationBatchResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string? status,
        int? page,
        int? pageSize,
        HttpContext context,
        GetNotificationBatchesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(status, page ?? 1, pageSize ?? 20, cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        var totalPages = result.TotalItems == 0
            ? 0
            : checked((int)Math.Ceiling(result.TotalItems / (double)result.PageSize));
        return Results.Json(ApiResponseFactory.Success(
            result.Items.Select(NotificationResponseMapper.ToResponse).ToArray(),
            context.TraceIdentifier,
            new OffsetPaginationMeta(result.Page, result.PageSize, result.TotalItems, totalPages)));
    }
}
