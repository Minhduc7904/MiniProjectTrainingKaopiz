using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.Quizzes;
using CourseService.Api.Mappers;
using CourseService.Application.Services.Content;
using CourseService.Application.UseCases.Quizzes.GetList;

namespace CourseService.Api.Endpoints.Quizzes.GetQuizzes;

public static class GetQuizzesEndpoint
{
    public static RouteHandlerBuilder MapGetQuizzes(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Courses.QuizzesTemplate,
                async (
                    string courseId,
                    string lessonId,
                    HttpContext context,
                    GetQuizzesHandler handler,
                    IMarkdownHtmlRenderer renderer,
                    CancellationToken cancellationToken) =>
                {
                    var quizzes = await handler.HandleAsync(
                        QuizRouteIds.Require(courseId),
                        QuizRouteIds.Require(lessonId),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(ApiResponseFactory.Success(QuizResponseMapper.ToResponse(quizzes, renderer), context.TraceIdentifier));
                })
            .WithName("get-lesson-quizzes")
            .WithTags(ServiceNames.Course)
            .Produces<ApiResponse<IReadOnlyList<QuizSummaryResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Admin);
}
