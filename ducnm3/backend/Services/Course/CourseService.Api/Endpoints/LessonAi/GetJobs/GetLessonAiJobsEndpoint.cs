// File: backend/Services/Course/CourseService.Api/Endpoints/LessonAi/GetJobs/GetLessonAiJobsEndpoint.cs
// Mục đích: GET các job AI gần nhất của Lesson (Admin).

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.LessonAi;
using CourseService.Api.Endpoints.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.UseCases.LessonAi.Jobs;

namespace CourseService.Api.Endpoints.LessonAi.GetJobs;

public static class GetLessonAiJobsEndpoint
{
    public static RouteHandlerBuilder MapGetLessonAiJobs(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Courses.LessonAiJobsTemplate,
                async (string courseId, string lessonId, HttpContext context, GetLessonAiJobsHandler handler, CancellationToken cancellationToken) =>
                {
                    var jobs = await handler.HandleListAsync(QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(ApiResponseFactory.Success(LessonAiResponseMapper.ToResponse(jobs), context.TraceIdentifier));
                })
            .WithName("get-lesson-ai-jobs")
            .WithTags(ServiceNames.Course)
            .Produces<ApiResponse<LessonAiJobsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Admin);
}
