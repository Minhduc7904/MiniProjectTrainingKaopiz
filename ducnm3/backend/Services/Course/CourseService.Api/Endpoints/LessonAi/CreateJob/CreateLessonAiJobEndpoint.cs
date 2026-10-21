// File: backend/Services/Course/CourseService.Api/Endpoints/LessonAi/CreateJob/CreateLessonAiJobEndpoint.cs
// Mục đích: POST tạo job AI (tổng hợp bài học hoặc soạn quiz) cho Lesson; trả 202 Accepted vì worker nền chạy job (Admin).

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.LessonAi;
using CourseService.Api.Endpoints.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.UseCases.LessonAi.Jobs;

namespace CourseService.Api.Endpoints.LessonAi.CreateJob;

public static class CreateLessonAiJobEndpoint
{
    public static RouteHandlerBuilder MapCreateLessonAiJob(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Courses.LessonAiJobsTemplate,
                async (
                    string courseId,
                    string lessonId,
                    CreateLessonAiJobRequest request,
                    HttpContext context,
                    CreateLessonAiJobHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var parsedCourseId = QuizRouteIds.Require(courseId);
                    var parsedLessonId = QuizRouteIds.Require(lessonId);
                    var job = await handler.HandleAsync(
                        new CreateLessonAiJobCommand(
                            parsedCourseId,
                            parsedLessonId,
                            context.GetRequiredActor().Id,
                            request.JobType,
                            request.MediaIds,
                            request.QuestionCount,
                            request.OverwriteContent ?? false),
                        cancellationToken);
                    context.Response.Headers.Location = ApiRoutes.Courses.LessonAiJobByIdPublicPath(parsedCourseId, parsedLessonId, job.Id);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(
                        ApiResponseFactory.Success(LessonAiResponseMapper.ToResponse(job), context.TraceIdentifier),
                        statusCode: StatusCodes.Status202Accepted);
                })
            .WithName("create-lesson-ai-job")
            .WithTags(ServiceNames.Course)
            .Accepts<CreateLessonAiJobRequest>("application/json")
            .Produces<ApiResponse<LessonAiJobResponse>>(StatusCodes.Status202Accepted)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireActor(ActorAccess.Admin);
}
