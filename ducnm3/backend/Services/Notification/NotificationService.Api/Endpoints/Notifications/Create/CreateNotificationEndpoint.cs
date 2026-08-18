// File: backend/Services/Notification/NotificationService.Api/Endpoints/Notifications/Create/CreateNotificationEndpoint.cs
// Mục đích: Map riêng POST tạo Notification trực tiếp, trả 201 và Location của notification vừa tạo.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using NotificationService.Api.Contracts.Notifications.Requests;
using NotificationService.Api.Contracts.Notifications.Responses;
using NotificationService.Api.Mappers;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.Notifications.Create;

namespace NotificationService.Api.Endpoints.Notifications.Create;

public static class CreateNotificationEndpoint
{
    public static IEndpointRouteBuilder MapCreateNotificationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(ApiRoutes.Notifications.Items, HandleAsync)
            .WithName("create-notification")
            .WithTags(ServiceNames.Notification)
            .Accepts<CreateNotificationRequest>("application/json")
            .Produces<ApiResponse<NotificationResponse>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateNotificationRequest request, HttpContext context, CreateNotificationHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId) || studentId == Guid.Empty ||
            !Guid.TryParse(request.CreatedBy, out var createdBy) || createdBy == Guid.Empty)
        {
            throw NotificationErrors.Validation("studentId and createdBy must be valid UUIDs.");
        }
        var result = await handler.HandleAsync(
            new CreateNotificationCommand(studentId, request.Title, request.BodyMarkdown, createdBy),
            cancellationToken);
        context.Response.Headers.Location = ApiRoutes.Notifications.ItemByIdPublicPath(result.Id);
        return Results.Json(ApiResponseFactory.Success(
            NotificationResponseMapper.ToResponse(result), context.TraceIdentifier),
            statusCode: StatusCodes.Status201Created);
    }
}
