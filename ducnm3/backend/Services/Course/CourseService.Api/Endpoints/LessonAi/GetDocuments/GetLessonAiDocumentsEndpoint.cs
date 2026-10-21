// File: backend/Services/Course/CourseService.Api/Endpoints/LessonAi/GetDocuments/GetLessonAiDocumentsEndpoint.cs
// Mục đích: GET danh sách tài liệu của Lesson kèm trạng thái trích xuất văn bản (Admin).

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.LessonAi;
using CourseService.Api.Endpoints.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.UseCases.LessonAi.Documents;

namespace CourseService.Api.Endpoints.LessonAi.GetDocuments;

public static class GetLessonAiDocumentsEndpoint
{
    public static RouteHandlerBuilder MapGetLessonAiDocuments(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Courses.LessonAiDocumentsTemplate,
                async (string courseId, string lessonId, HttpContext context, GetLessonAiDocumentsHandler handler, CancellationToken cancellationToken) =>
                {
                    var documents = await handler.HandleAsync(QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(ApiResponseFactory.Success(LessonAiResponseMapper.ToResponse(documents), context.TraceIdentifier));
                })
            .WithName("get-lesson-ai-documents")
            .WithTags(ServiceNames.Course)
            .Produces<ApiResponse<LessonAiDocumentsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Admin);
}
