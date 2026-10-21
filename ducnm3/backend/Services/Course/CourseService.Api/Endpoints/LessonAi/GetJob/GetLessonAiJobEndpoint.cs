// File: backend/Services/Course/CourseService.Api/Endpoints/LessonAi/GetJob/GetLessonAiJobEndpoint.cs
// Mục đích: GET chi tiết một job AI của Lesson, kèm nội dung Lesson cũ nếu job đã ghi đè (Admin).

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.LessonAi;
using CourseService.Api.Endpoints.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.UseCases.LessonAi.Jobs;

namespace CourseService.Api.Endpoints.LessonAi.GetJob;

public static class GetLessonAiJobEndpoint
{
    public static RouteHandlerBuilder MapGetLessonAiJob(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Courses.LessonAiJobByIdTemplate,
                async (string courseId, string lessonId, string jobId, HttpContext context, GetLessonAiJobsHandler handler, CancellationToken cancellationToken) =>
                {
                    var detail = await handler.HandleDetailAsync(
                        QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), QuizRouteIds.Require(jobId), cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(ApiResponseFactory.Success(LessonAiResponseMapper.ToResponse(detail), context.TraceIdentifier));
                })
            .WithName("get-lesson-ai-job")
            .WithTags(ServiceNames.Course)
            .Produces<ApiResponse<LessonAiJobDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Admin);
}
