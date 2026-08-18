// File: backend/Services/Notification/NotificationService.Api/Endpoints/Notifications/NotificationEndpoints.cs
// Mục đích: Khai báo route tạo và tra cứu notification đơn lẻ, chuyển DTO request sang use case và trả envelope chuẩn.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.Requests;
using NotificationService.Api.Contracts.Responses;
using NotificationService.Application;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Features.Notifications.Create;
using NotificationService.Application.Features.Notifications.GetById;

namespace NotificationService.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                ApiRoutes.Notifications.Items,
                async (
                    CreateNotificationRequest request,
                    HttpContext context,
                    CreateNotificationHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(request.StudentId, out var studentId) ||
                        studentId == Guid.Empty ||
                        !Guid.TryParse(request.CreatedBy, out var createdBy) ||
                        createdBy == Guid.Empty)
                    {
                        throw NotificationErrors.Validation(
                            "studentId and createdBy must be valid UUIDs.");
                    }

                    var result = await handler.HandleAsync(
                        new CreateNotificationCommand(
                            studentId,
                            request.Title,
                            request.BodyMarkdown,
                            createdBy),
                        cancellationToken);
                    context.Response.Headers.Location =
                        ApiRoutes.Notifications.ItemByIdPublicPath(result.Id);
                    return Results.Json(
                        ApiResponseFactory.Success(
                            ToResponse(result),
                            context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("create-notification")
            .WithTags(ServiceNames.Notification)
            .Accepts<CreateNotificationRequest>("application/json")
            .Produces<ApiResponse<NotificationResponse>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet(
                ApiRoutes.Notifications.ItemByIdTemplate,
                async (
                    string notificationId,
                    HttpContext context,
                    GetNotificationByIdHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(notificationId, out var id) || id == Guid.Empty)
                    {
                        throw NotificationErrors.Validation(
                            "notificationId must be a valid UUID.");
                    }

                    var result = await handler.HandleAsync(id, cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(
                        ApiResponseFactory.Success(
                            ToResponse(result),
                            context.TraceIdentifier));
                })
            .WithName("get-notification-by-id")
            .WithTags(ServiceNames.Notification)
            .Produces<ApiResponse<NotificationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static NotificationResponse ToResponse(NotificationSummary item) =>
        new(
            item.Id,
            item.RecipientStudentId,
            item.Title,
            item.BodyMarkdown,
            item.SourceType,
            item.Status,
            item.CreatedBy,
            item.CreatedAtUtc,
            item.ReadAtUtc);
}
