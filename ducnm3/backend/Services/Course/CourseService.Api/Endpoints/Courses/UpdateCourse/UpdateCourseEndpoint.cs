using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Api.Contracts.Courses;
using CourseService.Application.UseCases.Courses.Update;
using static CourseService.Api.Endpoints.Courses.CreateCourse.CreateCourseEndpoint;

namespace CourseService.Api.Endpoints.Courses.UpdateCourse;

public static class UpdateCourseEndpoint
{
    public static RouteHandlerBuilder MapUpdateCourse(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut(ApiRoutes.Courses.ByIdTemplate, async (string courseId, JsonElement request, HttpContext context, UpdateCourseHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var id) || id == Guid.Empty) throw new ArgumentException("courseId must be a valid UUID.");
            var command = new UpdateCourseCommand(id, TryString(request, "name", out var name), name, TryString(request, "descriptionMarkdown", out var description), description, TryString(request, "status", out var status), status, ReadActor(context.Request));
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Json(ApiResponseFactory.Success(new CourseCommandResponse(result.Id, result.Name, result.DescriptionMarkdown, result.Status, result.CreatedAtUtc, result.UpdatedAtUtc), context.TraceIdentifier));
        }).WithName("update-course").WithTags(ServiceNames.Course).Accepts<JsonElement>("application/json").Produces<ApiResponse<CourseCommandResponse>>(StatusCodes.Status200OK);

    internal static bool TryString(JsonElement request, string name, out string? value)
    {
        if (!request.TryGetProperty(name, out var property)) { value = null; return false; }
        value = property.ValueKind == JsonValueKind.Null ? null : property.GetString();
        return true;
    }
}
