// File: backend/Services/Course/CourseService.Api/Endpoints/Learning/StudentQuizEndpoints.cs
// Mục đích: Endpoint học viên làm Quiz: xem đề, bắt đầu/tiếp tục, lưu nháp, nộp bài và xem lại các lần làm.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.Quizzes;
using CourseService.Api.Endpoints.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.Services.Content;
using CourseService.Application.UseCases.Learning.Quizzes;

namespace CourseService.Api.Endpoints.Learning;

public static class StudentQuizEndpoints
{
    public static RouteHandlerBuilder MapGetStudentQuizzes(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.StudentQuizzesTemplate, async (
                string courseId, string lessonId, HttpContext context, GetStudentQuizzesHandler handler,
                IMarkdownHtmlRenderer renderer, CancellationToken cancellationToken) =>
            {
                var quizzes = await handler.HandleAsync(
                    QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), context.GetRequiredActor().Id, cancellationToken);
                return NoStoreJson(context, QuizResponseMapper.ToStudentResponse(quizzes, renderer));
            })
            .WithName("get-student-lesson-quizzes").WithTags(ServiceNames.Course)
            .Produces<ApiResponse<IReadOnlyList<StudentQuizSummaryResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapGetStudentQuizDetail(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.StudentQuizByIdTemplate, async (
                string courseId, string lessonId, string quizId, HttpContext context, GetStudentQuizDetailHandler handler,
                IMarkdownHtmlRenderer renderer, CancellationToken cancellationToken) =>
            {
                var quiz = await handler.HandleAsync(
                    QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), QuizRouteIds.Require(quizId),
                    context.GetRequiredActor().Id, cancellationToken);
                return NoStoreJson(context, QuizResponseMapper.ToStudentResponse(quiz, renderer));
            })
            .WithName("get-student-lesson-quiz-detail").WithTags(ServiceNames.Course)
            .Produces<ApiResponse<StudentQuizResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapStartQuizAttempt(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Courses.StudentQuizAttemptsTemplate, async (
                string courseId, string lessonId, string quizId, HttpContext context, StartQuizAttemptHandler handler,
                CancellationToken cancellationToken) =>
            {
                var parsedCourseId = QuizRouteIds.Require(courseId);
                var parsedLessonId = QuizRouteIds.Require(lessonId);
                var parsedQuizId = QuizRouteIds.Require(quizId);
                var result = await handler.HandleAsync(
                    parsedCourseId, parsedLessonId, parsedQuizId, context.GetRequiredActor().Id, cancellationToken);
                context.Response.Headers.Location = ApiRoutes.Courses.StudentQuizAttemptByIdPublicPath(
                    parsedCourseId, parsedLessonId, parsedQuizId, result.Attempt.Id);
                return NoStoreJson(
                    context,
                    QuizResponseMapper.ToResponse(result.Attempt),
                    result.Created ? StatusCodes.Status201Created : StatusCodes.Status200OK);
            })
            .WithName("start-student-quiz-attempt").WithTags(ServiceNames.Course)
            .Produces<ApiResponse<QuizAttemptResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<QuizAttemptResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapSaveQuizAnswers(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut(ApiRoutes.Courses.StudentQuizAttemptAnswersTemplate, async (
                string courseId, string lessonId, string quizId, string attemptId, SaveQuizAnswersRequest request,
                HttpContext context, SaveQuizAnswersHandler handler, CancellationToken cancellationToken) =>
            {
                var attempt = await handler.HandleAsync(
                    QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), QuizRouteIds.Require(quizId),
                    QuizRouteIds.Require(attemptId), context.GetRequiredActor().Id,
                    QuizRequestMapper.ToAnswers(request.Answers), cancellationToken);
                return NoStoreJson(context, QuizResponseMapper.ToResponse(attempt));
            })
            .WithName("save-student-quiz-answers").WithTags(ServiceNames.Course)
            .Accepts<SaveQuizAnswersRequest>("application/json")
            .Produces<ApiResponse<QuizAttemptResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapSubmitQuizAttempt(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Courses.StudentQuizAttemptSubmitTemplate, async (
                string courseId, string lessonId, string quizId, string attemptId, SubmitQuizAttemptRequest? request,
                HttpContext context, SubmitQuizAttemptHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), QuizRouteIds.Require(quizId),
                    QuizRouteIds.Require(attemptId), context.GetRequiredActor().Id,
                    QuizRequestMapper.ToAnswers(request?.Answers), cancellationToken);
                return NoStoreJson(context, QuizResponseMapper.ToDetailResponse(result.Attempt, result.Grade));
            })
            .WithName("submit-student-quiz-attempt").WithTags(ServiceNames.Course)
            .Accepts<SubmitQuizAttemptRequest>("application/json")
            .Produces<ApiResponse<QuizAttemptDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapGetQuizAttempts(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.StudentQuizAttemptsTemplate, async (
                string courseId, string lessonId, string quizId, HttpContext context, GetQuizAttemptsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var attempts = await handler.HandleAsync(
                    QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), QuizRouteIds.Require(quizId),
                    context.GetRequiredActor().Id, cancellationToken);
                return NoStoreJson(context, QuizResponseMapper.ToResponse(attempts));
            })
            .WithName("get-student-quiz-attempts").WithTags(ServiceNames.Course)
            .Produces<ApiResponse<IReadOnlyList<QuizAttemptSummaryResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapGetQuizAttemptDetail(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.StudentQuizAttemptByIdTemplate, async (
                string courseId, string lessonId, string quizId, string attemptId, HttpContext context,
                GetQuizAttemptDetailHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    QuizRouteIds.Require(courseId), QuizRouteIds.Require(lessonId), QuizRouteIds.Require(quizId),
                    QuizRouteIds.Require(attemptId), context.GetRequiredActor().Id, cancellationToken);
                return NoStoreJson(context, QuizResponseMapper.ToDetailResponse(result.Attempt, result.Grade));
            })
            .WithName("get-student-quiz-attempt-detail").WithTags(ServiceNames.Course)
            .Produces<ApiResponse<QuizAttemptDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Student);

    private static IResult NoStoreJson<T>(HttpContext context, T data, int statusCode = StatusCodes.Status200OK)
    {
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(ApiResponseFactory.Success(data, context.TraceIdentifier), statusCode: statusCode);
    }
}
