using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Api.Contracts.Courses;
using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Courses.Create;

namespace CourseService.Api.Endpoints.Courses.CreateCourse;

public static class CreateCourseEndpoint
{
    public static RouteHandlerBuilder MapCreateCourse(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Courses.List, async (CreateCourseRequest request, HttpContext context, CreateCourseHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request.Name, request.DescriptionMarkdown, ReadActor(context.Request), cancellationToken);
            context.Response.Headers.Location = ApiRoutes.Courses.DetailsPublicPath(result.Id);
            return Results.Json(ApiResponseFactory.Success(new CourseCommandResponse(result.Id, result.Name, result.DescriptionMarkdown, result.Status, result.CreatedAtUtc, result.UpdatedAtUtc), context.TraceIdentifier), statusCode: StatusCodes.Status201Created);
        }).WithName("create-course").WithTags(ServiceNames.Course).Accepts<CreateCourseRequest>("application/json").Produces<ApiResponse<CourseCommandResponse>>(StatusCodes.Status201Created).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

    internal static Guid ReadActor(HttpRequest request)
    {
        return Guid.TryParse(request.Headers[ApiHeaderNames.ActorId], out var actor) && actor != Guid.Empty
            ? actor : throw CourseErrors.ValidationFailed([]);
    }
}

public sealed record CreateCourseRequest(string? Name, string? DescriptionMarkdown);
