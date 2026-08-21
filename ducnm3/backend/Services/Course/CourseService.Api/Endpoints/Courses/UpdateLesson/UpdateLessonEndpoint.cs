using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Api.Contracts.Courses;
using CourseService.Application.UseCases.Lessons.Update;
using static CourseService.Api.Endpoints.Courses.CreateCourse.CreateCourseEndpoint;
using static CourseService.Api.Endpoints.Courses.UpdateCourse.UpdateCourseEndpoint;

namespace CourseService.Api.Endpoints.Courses.UpdateLesson;

public static class UpdateLessonEndpoint
{
    public static RouteHandlerBuilder MapUpdateLesson(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut(ApiRoutes.Courses.LessonByIdTemplate, async (string courseId, string lessonId, JsonElement request, HttpContext context, UpdateLessonHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty || !Guid.TryParse(lessonId, out var parsedLessonId) || parsedLessonId == Guid.Empty) throw new ArgumentException("Route IDs must be valid UUIDs.");
            var command = new UpdateLessonCommand(parsedCourseId, parsedLessonId, TryString(request, "title", out var title), title, TryString(request, "contentMarkdown", out var content), content, ReadActor(context.Request));
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Json(ApiResponseFactory.Success(new LessonCommandResponse(result.Id, result.CourseId, result.Title, result.ContentMarkdown, null, result.DisplayOrder, result.CreatedAtUtc, result.UpdatedAtUtc, []), context.TraceIdentifier));
        }).WithName("update-course-lesson").WithTags(ServiceNames.Course).Accepts<JsonElement>("application/json").Produces<ApiResponse<LessonCommandResponse>>(StatusCodes.Status200OK);
}
