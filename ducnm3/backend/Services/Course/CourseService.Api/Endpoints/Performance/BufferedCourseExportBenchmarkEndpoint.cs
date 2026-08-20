using BuildingBlocks.Contracts.Api;
using CourseService.Application.UseCases.Courses.Export;

namespace CourseService.Api.Endpoints.Performance;

public static class BufferedCourseExportBenchmarkEndpoint
{
    private const string ContentDisposition = "attachment; filename=\"courses.csv\"";

    public static RouteHandlerBuilder MapBufferedCourseExportBenchmark(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.BufferedExportBenchmark, async (
                string? status,
            int? limit,
                HttpContext context,
                BufferedCourseExportHandler handler,
                CancellationToken cancellationToken) =>
            {
                var bytes = await handler.HandleAsync(
                    ExportCoursesQuery.Create(status, limit),
                    cancellationToken);
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/csv; charset=utf-8";
                context.Response.Headers.ContentDisposition = ContentDisposition;
                context.Response.Headers.CacheControl = "no-store";
                await context.Response.Body.WriteAsync(bytes, cancellationToken);
            })
            .WithName("benchmark-buffered-course-export")
            .WithTags(ServiceNames.Course)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK, contentType: "text/csv")
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
}
