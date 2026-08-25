using BuildingBlocks.Contracts.Api;
using CourseService.Application.UseCases.Courses.Export;

namespace CourseService.Api.Endpoints.Performance;

public static class BufferedCourseExportBenchmarkEndpoint
{
    // Endpoint benchmark cố tình so sánh với streaming export; không public trong OpenAPI.
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
                // Handler trả byte[] sau khi đã đọc và serialize toàn bộ CSV trong MemoryStream.
                // Vì vậy endpoint này có peak memory theo kích thước file, chỉ dùng để đo chênh lệch.
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
