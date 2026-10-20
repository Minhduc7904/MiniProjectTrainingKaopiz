using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.Services.Content;
using CourseService.Application.UseCases.Quizzes;
using CourseService.Application.UseCases.Quizzes.Create;

namespace CourseService.Api.Endpoints.Quizzes.CreateQuiz;

public static class CreateQuizEndpoint
{
    public static RouteHandlerBuilder MapCreateQuiz(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Courses.QuizzesTemplate,
                async (
                    string courseId,
                    string lessonId,
                    CreateQuizRequest request,
                    HttpContext context,
                    CreateQuizHandler handler,
                    IMarkdownHtmlRenderer renderer,
                    CancellationToken cancellationToken) =>
                {
                    var parsedCourseId = QuizRouteIds.Require(courseId);
                    var parsedLessonId = QuizRouteIds.Require(lessonId);
                    var quiz = await handler.HandleAsync(
                        new CreateQuizCommand(
                            parsedCourseId,
                            parsedLessonId,
                            context.GetRequiredActor().Id,
                            QuizRequestMapper.ToInput(request.Title, request.DescriptionMarkdown, request.Status, request.Questions)),
                        cancellationToken);
                    context.Response.Headers.Location = ApiRoutes.Courses.QuizByIdPublicPath(parsedCourseId, parsedLessonId, quiz.Id);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(
                        ApiResponseFactory.Success(QuizResponseMapper.ToResponse(quiz, renderer), context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("create-lesson-quiz")
            .WithTags(ServiceNames.Course)
            .Accepts<CreateQuizRequest>("application/json")
            .Produces<ApiResponse<QuizResponse>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Admin);
}
