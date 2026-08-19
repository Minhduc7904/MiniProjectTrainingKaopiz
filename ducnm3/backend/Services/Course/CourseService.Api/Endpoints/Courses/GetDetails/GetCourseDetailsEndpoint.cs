using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Courses.GetDetails;

namespace CourseService.Api.Endpoints.Courses.GetDetails;

public static class GetCourseDetailsEndpoint
{
    public static RouteHandlerBuilder MapGetCourseDetails(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Courses.DetailsTemplate,
                async (
                    string courseId,
                    HttpContext context,
                    GetCourseDetailsHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty)
                    {
                        throw CourseErrors.ValidationFailed(
                        [
                            new ApiErrorDetail("courseId", "courseId must be a valid UUID.")
                        ]);
                    }

                    var result = await handler.HandleAsync(parsedCourseId, cancellationToken);
                    return Results.Ok(ApiResponseFactory.Success(result, context.TraceIdentifier));
                })
            .WithName("get-course-details")
            .WithTags(ServiceNames.Course)
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
