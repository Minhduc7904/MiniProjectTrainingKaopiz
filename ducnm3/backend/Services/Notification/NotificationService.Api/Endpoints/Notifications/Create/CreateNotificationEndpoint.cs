// File: backend/Services/Notification/NotificationService.Api/Endpoints/Notifications/Create/CreateNotificationEndpoint.cs
// Mục đích: Map riêng POST tạo Notification trực tiếp, trả 201 và Location của notification vừa tạo.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using NotificationService.Api.Contracts.Notifications.Requests;
using NotificationService.Api.Contracts.Notifications.Responses;
using NotificationService.Api.Mappers;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.Notifications.Create;

namespace NotificationService.Api.Endpoints.Notifications.Create;

public static class CreateNotificationEndpoint
{
    public static RouteHandlerBuilder MapCreateNotificationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost(ApiRoutes.Notifications.Items, HandleAsync)
            .WithName("create-notification")
            .WithTags(ServiceNames.Notification)
            .Accepts<CreateNotificationRequest>("application/json")
            .Produces<ApiResponse<NotificationResponse>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable)
            .RequireActor(ActorAccess.Admin);
    }

    private static async Task<IResult> HandleAsync(
        CreateNotificationRequest request, HttpContext context, CreateNotificationHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId) || studentId == Guid.Empty)
        {
            throw NotificationErrors.Validation("studentId must be a valid UUID.");
        }
        var result = await handler.HandleAsync(
            new CreateNotificationCommand(studentId, request.Title, request.BodyMarkdown, context.GetRequiredActor().Id),
            cancellationToken);
        context.Response.Headers.Location = ApiRoutes.Notifications.ItemByIdPublicPath(result.Id);
        return Results.Json(ApiResponseFactory.Success(
            NotificationResponseMapper.ToResponse(result), context.TraceIdentifier),
            statusCode: StatusCodes.Status201Created);
    }
}
