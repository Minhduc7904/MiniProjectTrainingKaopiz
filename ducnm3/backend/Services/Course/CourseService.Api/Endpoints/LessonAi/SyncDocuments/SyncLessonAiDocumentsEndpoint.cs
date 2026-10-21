// File: backend/Services/Course/CourseService.Api/Endpoints/LessonAi/SyncDocuments/SyncLessonAiDocumentsEndpoint.cs
// Mục đích: POST đồng bộ tệp đính kèm của Lesson vào hàng chờ trích xuất văn bản; retryFailed=true thử lại tài liệu lỗi (Admin).

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.LessonAi;
using CourseService.Api.Endpoints.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.UseCases.LessonAi.Documents;

namespace CourseService.Api.Endpoints.LessonAi.SyncDocuments;

public static class SyncLessonAiDocumentsEndpoint
{
    public static RouteHandlerBuilder MapSyncLessonAiDocuments(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Courses.LessonAiDocumentsSyncTemplate,
                async (
                    string courseId,
                    string lessonId,
                    bool? retryFailed,
                    HttpContext context,
                    SyncLessonAiDocumentsHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var documents = await handler.HandleAsync(
                        new SyncLessonAiDocumentsCommand(
                            QuizRouteIds.Require(courseId),
                            QuizRouteIds.Require(lessonId),
                            context.GetRequiredActor().Id,
                            retryFailed ?? false),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(ApiResponseFactory.Success(LessonAiResponseMapper.ToResponse(documents), context.TraceIdentifier));
                })
            .WithName("sync-lesson-ai-documents")
            .WithTags(ServiceNames.Course)
            .Produces<ApiResponse<LessonAiDocumentsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Admin);
}
