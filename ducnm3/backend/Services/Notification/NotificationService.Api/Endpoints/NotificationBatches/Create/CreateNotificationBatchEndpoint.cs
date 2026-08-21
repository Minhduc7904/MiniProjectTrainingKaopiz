// File: backend/Services/Notification/NotificationService.Api/Endpoints/NotificationBatches/Create/CreateNotificationBatchEndpoint.cs
// Mục đích: Map riêng POST tạo Notification Batch, trả 202 và Location để client theo dõi trạng thái batch.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using NotificationService.Api.Contracts.NotificationBatches.Requests;
using NotificationService.Api.Contracts.NotificationBatches.Responses;
using NotificationService.Api.Mappers;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.NotificationBatches.Create;

namespace NotificationService.Api.Endpoints.NotificationBatches.Create;

public static class CreateNotificationBatchEndpoint
{
    public static RouteHandlerBuilder MapCreateNotificationBatchEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost(ApiRoutes.Notifications.Batches, HandleAsync)
            .WithName("create-notification-batch")
            .WithTags(ServiceNames.Notification)
            .Accepts<CreateNotificationBatchRequest>("application/json")
            .Produces<ApiResponse<NotificationBatchResponse>>(StatusCodes.Status202Accepted)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable)
            .RequireActor(ActorAccess.Admin);
    }

    private static async Task<IResult> HandleAsync(
        CreateNotificationBatchRequest request,
        HttpContext context,
        CreateNotificationBatchHandler handler,
        CancellationToken cancellationToken)
    {
        Guid? courseId = null;
        if (!string.IsNullOrWhiteSpace(request.CourseId))
        {
            if (!Guid.TryParse(request.CourseId, out var parsed))
            {
                throw NotificationErrors.Validation("courseId must be a valid UUID.");
            }
            courseId = parsed;
        }

        var result = await handler.HandleAsync(
            new CreateNotificationBatchCommand(request.Title, request.BodyMarkdown,
                request.TargetScope, context.GetRequiredActor().Id, request.BatchSize, request.RequestedCount, courseId),
            cancellationToken);
        context.Response.Headers.Location = ApiRoutes.Notifications.BatchByIdPublicPath(result.Id);
        return Results.Json(
            ApiResponseFactory.Success(NotificationResponseMapper.ToResponse(result), context.TraceIdentifier),
            statusCode: StatusCodes.Status202Accepted);
    }
}
