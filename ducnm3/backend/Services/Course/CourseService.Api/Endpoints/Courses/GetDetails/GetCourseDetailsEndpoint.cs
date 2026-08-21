using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Courses.GetDetails;
using CourseService.Application.Services.Media;
using CourseService.Application.Services.Content;
using CourseService.Api.Contracts.Courses;
using CourseService.Api.Mappers;

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
                    ICourseMediaReader mediaReader,
                    IMarkdownHtmlRenderer markdownRenderer,
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
                    var media = await mediaReader.GetAsync(parsedCourseId, cancellationToken);
                    return Results.Ok(ApiResponseFactory.Success(
                        CourseResponseMapper.ToDetailsResponse(result, media, markdownRenderer),
                        context.TraceIdentifier));
                })
            .WithName("get-course-details")
            .WithTags(ServiceNames.Course)
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
