using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.Services.Content;
using CourseService.Application.UseCases.Quizzes.Update;

namespace CourseService.Api.Endpoints.Quizzes.UpdateQuiz;

public static class UpdateQuizEndpoint
{
    public static RouteHandlerBuilder MapUpdateQuiz(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut(
                ApiRoutes.Courses.QuizByIdTemplate,
                async (
                    string courseId,
                    string lessonId,
                    string quizId,
                    UpdateQuizRequest request,
                    HttpContext context,
                    UpdateQuizHandler handler,
                    IMarkdownHtmlRenderer renderer,
                    CancellationToken cancellationToken) =>
                {
                    var quiz = await handler.HandleAsync(
                        new UpdateQuizCommand(
                            QuizRouteIds.Require(courseId),
                            QuizRouteIds.Require(lessonId),
                            QuizRouteIds.Require(quizId),
                            context.GetRequiredActor().Id,
                            QuizRequestMapper.ToInput(request.Title, request.DescriptionMarkdown, request.Status, request.Questions)),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(ApiResponseFactory.Success(QuizResponseMapper.ToResponse(quiz, renderer), context.TraceIdentifier));
                })
            .WithName("update-lesson-quiz")
            .WithTags(ServiceNames.Course)
            .Accepts<UpdateQuizRequest>("application/json")
            .Produces<ApiResponse<QuizResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireActor(ActorAccess.Admin);
}
