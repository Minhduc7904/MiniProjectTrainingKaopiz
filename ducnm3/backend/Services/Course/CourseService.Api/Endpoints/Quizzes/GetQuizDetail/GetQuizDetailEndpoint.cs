using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.Services.Content;
using CourseService.Application.UseCases.Quizzes.GetDetail;

namespace CourseService.Api.Endpoints.Quizzes.GetQuizDetail;

public static class GetQuizDetailEndpoint
{
    public static RouteHandlerBuilder MapGetQuizDetail(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Courses.QuizByIdTemplate,
                async (
                    string courseId,
                    string lessonId,
                    string quizId,
                    HttpContext context,
                    GetQuizDetailHandler handler,
                    IMarkdownHtmlRenderer renderer,
                    CancellationToken cancellationToken) =>
                {
                    var quiz = await handler.HandleAsync(
                        QuizRouteIds.Require(courseId),
                        QuizRouteIds.Require(lessonId),
                        QuizRouteIds.Require(quizId),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(ApiResponseFactory.Success(QuizResponseMapper.ToResponse(quiz, renderer), context.TraceIdentifier));
                })
            .WithName("get-lesson-quiz-detail")
            .WithTags(ServiceNames.Course)
            .Produces<ApiResponse<QuizResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Admin);
}
