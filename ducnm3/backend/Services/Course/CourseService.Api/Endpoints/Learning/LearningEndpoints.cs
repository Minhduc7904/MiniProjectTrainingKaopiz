using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Learning;

namespace CourseService.Api.Endpoints.Learning;

public static class LearningEndpoints
{
    public static RouteHandlerBuilder MapEnrollCourse(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Courses.EnrollmentsTemplate, async (string courseId, HttpContext context, EnrollCourseHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId)) throw CourseErrors.ValidationFailed([]);
            var result = await handler.HandleAsync(parsedCourseId, context.GetRequiredActor().Id, cancellationToken);
            return Results.Json(ApiResponseFactory.Success(result, context.TraceIdentifier), statusCode: StatusCodes.Status201Created);
        }).WithName("enroll-course").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<EnrollmentResult>>(StatusCodes.Status201Created)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
          .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
          .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapCompleteLessonProgress(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Courses.CompleteLessonProgressTemplate, async (string courseId, string lessonId, HttpContext context, CompleteLessonProgressHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId) || !Guid.TryParse(lessonId, out var parsedLessonId)) throw CourseErrors.ValidationFailed([]);
            var result = await handler.HandleAsync(parsedCourseId, parsedLessonId, context.GetRequiredActor().Id, cancellationToken);
            return Results.Json(ApiResponseFactory.Success(result, context.TraceIdentifier));
        }).WithName("complete-lesson-progress").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<LessonProgressResult>>(StatusCodes.Status200OK)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
          .RequireActor(ActorAccess.Student);
}
