using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.Requests;
using NotificationService.Api.Contracts.Responses;
using NotificationService.Application;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Features.Batches.Create;
using NotificationService.Application.Features.Batches.GetById;
using NotificationService.Application.Features.Batches.GetFailedItems;

namespace NotificationService.Api.Endpoints;

public static class NotificationBatchEndpoints
{
    public static IEndpointRouteBuilder MapNotificationBatchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                ApiRoutes.Notifications.Batches,
                async (
                    CreateNotificationBatchRequest request,
                    HttpContext context,
                    CreateNotificationBatchHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(request.CreatedBy, out var createdBy) ||
                        createdBy == Guid.Empty)
                    {
                        throw NotificationErrors.Validation(
                            "createdBy must be a valid UUID.");
                    }

                    Guid? courseId = null;
                    if (!string.IsNullOrWhiteSpace(request.CourseId))
                    {
                        if (!Guid.TryParse(request.CourseId, out var parsed))
                        {
                            throw NotificationErrors.Validation(
                                "courseId must be a valid UUID.");
                        }

                        courseId = parsed;
                    }

                    var result = await handler.HandleAsync(
                        new CreateNotificationBatchCommand(
                            request.Title,
                            request.BodyMarkdown,
                            request.TargetScope,
                            createdBy,
                            request.BatchSize,
                            courseId),
                        cancellationToken);
                    context.Response.Headers.Location =
                        ApiRoutes.Notifications.BatchByIdPublicPath(result.Id);
                    return Results.Json(
                        ApiResponseFactory.Success(
                            ToResponse(result),
                            context.TraceIdentifier),
                        statusCode: StatusCodes.Status202Accepted);
                })
            .WithName("create-notification-batch")
            .WithTags(ServiceNames.Notification)
            .Accepts<CreateNotificationBatchRequest>("application/json")
            .Produces<ApiResponse<NotificationBatchResponse>>(
                StatusCodes.Status202Accepted)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);

        endpoints
            .MapGet(
                ApiRoutes.Notifications.BatchByIdTemplate,
                async (
                    string batchId,
                    HttpContext context,
                    GetNotificationBatchByIdHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(batchId, out var id) || id == Guid.Empty)
                    {
                        throw NotificationErrors.Validation(
                            "batchId must be a valid UUID.");
                    }

                    var result = await handler.HandleAsync(id, cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(
                        ApiResponseFactory.Success(
                            ToResponse(result),
                            context.TraceIdentifier));
                })
            .WithName("get-notification-batch-by-id")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationBatchResponse>>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        endpoints
            .MapGet(
                ApiRoutes.Notifications.BatchFailedItemsTemplate,
                async (
                    string batchId,
                    string? cursor,
                    int? limit,
                    HttpContext context,
                    GetNotificationBatchFailedItemsHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(batchId, out var id) || id == Guid.Empty)
                    {
                        throw NotificationErrors.Validation("batchId must be a valid UUID.");
                    }

                    var page = await handler.HandleAsync(
                        id,
                        cursor,
                        limit ?? 100,
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(
                        ApiResponseFactory.Success(
                            new NotificationBatchFailedItemsResponse(
                                page.Items.Select(x => new NotificationBatchFailedItemResponse(
                                    x.StudentId,
                                    x.RetryCount,
                                    x.ErrorMessage)).ToArray()),
                            context.TraceIdentifier,
                            new CursorPaginationMeta(
                                limit ?? 100,
                                GetNotificationBatchFailedItemsHandler.EncodeCursor(page.NextItemId),
                                page.HasNextPage)));
                })
            .WithName("get-notification-batch-failed-items")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationBatchFailedItemsResponse>>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static NotificationBatchResponse ToResponse(
        NotificationBatchSummary item) =>
        new(
            item.Id,
            item.Status,
            item.TotalCount,
            item.ProcessedCount,
            item.SuccessCount,
            item.FailedCount,
            item.BatchSize,
            item.CreatedAtUtc,
            item.StartedAtUtc,
            item.CompletedAtUtc);
}
