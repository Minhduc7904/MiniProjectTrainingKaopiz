using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Lessons.Delete;

namespace CourseService.Api.Endpoints.Courses.DeleteLesson;

public static class DeleteLessonEndpoint
{
    public static RouteHandlerBuilder MapDeleteLesson(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete(ApiRoutes.Courses.LessonByIdTemplate, async (
            string courseId, string lessonId, HttpContext context, DeleteLessonHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty ||
                !Guid.TryParse(lessonId, out var parsedLessonId) || parsedLessonId == Guid.Empty)
                throw CourseErrors.ValidationFailed([]);
            await handler.HandleAsync(parsedCourseId, parsedLessonId, context.GetRequiredActor().Id, cancellationToken);
            return Results.Accepted();
        }).WithName("delete-course-lesson").WithTags(ServiceNames.Course)
          .Produces(StatusCodes.Status202Accepted)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .RequireActor(ActorAccess.Admin);
}
