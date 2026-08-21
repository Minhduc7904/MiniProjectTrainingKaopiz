using BuildingBlocks.Contracts.Api;
using CourseService.Api.Endpoints.Courses.CreateCourse;
using CourseService.Application.UseCases.Courses.Delete;

namespace CourseService.Api.Endpoints.Courses.DeleteCourse;

public static class DeleteCourseEndpoint
{
    public static RouteHandlerBuilder MapDeleteCourse(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete(
                ApiRoutes.Courses.ByIdTemplate,
                async (
                    string courseId,
                    HttpContext context,
                    DeleteCourseHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(courseId, out var id) || id == Guid.Empty)
                    {
                        throw new ArgumentException("courseId must be a valid UUID.");
                    }

                    await handler.HandleAsync(id, ReadActor(context.Request), cancellationToken);
                    return Results.Accepted();
                })
            .WithName("delete-course")
            .WithTags(ServiceNames.Course)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
