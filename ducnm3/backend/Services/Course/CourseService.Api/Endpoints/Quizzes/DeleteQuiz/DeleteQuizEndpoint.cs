using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Application.UseCases.Quizzes.Delete;

namespace CourseService.Api.Endpoints.Quizzes.DeleteQuiz;

public static class DeleteQuizEndpoint
{
    public static RouteHandlerBuilder MapDeleteQuiz(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete(
                ApiRoutes.Courses.QuizByIdTemplate,
                async (
                    string courseId,
                    string lessonId,
                    string quizId,
                    HttpContext context,
                    DeleteQuizHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    await handler.HandleAsync(
                        QuizRouteIds.Require(courseId),
                        QuizRouteIds.Require(lessonId),
                        QuizRouteIds.Require(quizId),
                        context.GetRequiredActor().Id,
                        cancellationToken);
                    return Results.Accepted();
                })
            .WithName("delete-lesson-quiz")
            .WithTags(ServiceNames.Course)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Admin);
}
