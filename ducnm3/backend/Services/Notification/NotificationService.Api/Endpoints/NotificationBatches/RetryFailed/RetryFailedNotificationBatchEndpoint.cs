// File: backend/Services/Notification/NotificationService.Api/Endpoints/NotificationBatches/RetryFailed/RetryFailedNotificationBatchEndpoint.cs
// Mục đích: Map riêng POST retry recipient FAILED, trả 202 và Location của batch con idempotent để theo dõi tiến trình.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using NotificationService.Api.Contracts.NotificationBatches.Responses;
using NotificationService.Api.Mappers;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.NotificationBatches.RetryFailed;

namespace NotificationService.Api.Endpoints.NotificationBatches.RetryFailed;

public static class RetryFailedNotificationBatchEndpoint
{
    public static RouteHandlerBuilder MapRetryFailedNotificationBatchEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost(ApiRoutes.Notifications.BatchRetryFailedTemplate, HandleAsync)
            .WithName("retry-failed-notification-batch")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationBatchResponse>>(StatusCodes.Status202Accepted)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireActor(ActorAccess.Admin);
    }

    private static async Task<IResult> HandleAsync(
        string batchId,
        HttpContext context,
        RetryFailedNotificationBatchHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(batchId, out var id) || id == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId must be a valid UUID.");
        }

        var result = await handler.HandleAsync(
            id,
            context.GetRequiredActor().Id,
            cancellationToken);
        context.Response.Headers.Location = ApiRoutes.Notifications.BatchByIdPublicPath(result.Id);
        return Results.Json(
            ApiResponseFactory.Success(NotificationResponseMapper.ToResponse(result), context.TraceIdentifier),
            statusCode: StatusCodes.Status202Accepted);
    }
}
