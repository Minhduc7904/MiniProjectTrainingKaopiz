using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Api.Contracts.Courses;
using CourseService.Application.UseCases.Lessons.Create;

namespace CourseService.Api.Endpoints.Courses.CreateLesson;

public static class CreateLessonEndpoint
{
    public static RouteHandlerBuilder MapCreateLesson(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Courses.LessonsTemplate,
                async (
                    string courseId,
                    CreateLessonRequest request,
                    HttpContext context,
                    CreateLessonHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty)
                    {
                        return Results.BadRequest(ApiResponseFactory.Error(
                            "VALIDATION_FAILED",
                            "courseId must be a valid UUID.",
                            context.TraceIdentifier));
                    }

                    var result = await handler.HandleAsync(
                        parsedCourseId,
                        request.Title,
                        request.ContentMarkdown,
                        request.DisplayOrder,
                        ReadActor(context.Request),
                        cancellationToken);
                    var response = new CreateLessonResponse(
                        result.Id,
                        result.CourseId,
                        result.Title,
                        result.ContentMarkdown,
                        result.DisplayOrder,
                        result.CreatedAtUtc,
                        result.UpdatedAtUtc);
                    return Results.Json(
                        ApiResponseFactory.Success(response, context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("create-course-lesson")
            .WithTags(ServiceNames.Course)
            .Accepts<CreateLessonRequest>("application/json")
            .Produces<ApiResponse<CreateLessonResponse>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

    private static Guid ReadActor(HttpRequest request)
    {
        var actorType = request.Headers[ApiHeaderNames.ActorType].ToString();
        var actorIdText = request.Headers[ApiHeaderNames.ActorId].ToString();
        if (string.IsNullOrWhiteSpace(actorType) || !Guid.TryParse(actorIdText, out var actorId) || actorId == Guid.Empty)
        {
            throw new InvalidOperationException("Actor headers are required.");
        }
        return actorId;
    }
}
