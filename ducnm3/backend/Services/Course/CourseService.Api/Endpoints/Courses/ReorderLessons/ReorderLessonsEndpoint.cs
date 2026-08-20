using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Application.UseCases.Lessons.Reorder;

namespace CourseService.Api.Endpoints.Courses.ReorderLessons;

public static class ReorderLessonsEndpoint
{
    public static RouteHandlerBuilder MapReorderLessons(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut(ApiRoutes.Courses.LessonReorderTemplate, async (string courseId, ReorderLessonsRequest request, HttpContext context, ReorderLessonsHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty) throw new ArgumentException("courseId must be a valid UUID.");
            await handler.HandleAsync(parsedCourseId, request.LessonIds, cancellationToken);
            return Results.Json(ApiResponseFactory.Success(new { lessonIds = request.LessonIds }, context.TraceIdentifier));
        }).WithName("reorder-course-lessons").WithTags(ServiceNames.Course).Accepts<ReorderLessonsRequest>("application/json").Produces(StatusCodes.Status200OK);
}

public sealed record ReorderLessonsRequest(IReadOnlyList<Guid> LessonIds);
