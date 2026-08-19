// File: backend/Services/Course/CourseService.Api/Endpoints/Courses/Export/ExportCoursesEndpoint.cs
// Mục đích: Stream CSV Course theo chunk keyset trực tiếp vào HTTP response.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.UseCases.Courses.Export;

namespace CourseService.Api.Endpoints.Courses.Export;

public static class ExportCoursesEndpoint
{
    private const string ContentDisposition = "attachment; filename=\"courses.csv\"";

    public static RouteHandlerBuilder MapExportCourses(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.Export, async (
                string? status,
                HttpContext context,
                ExportCoursesHandler handler,
                CancellationToken cancellationToken) =>
            {
                var query = ExportCoursesQuery.Create(status);
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/csv; charset=utf-8";
                context.Response.Headers.ContentDisposition = ContentDisposition;
                context.Response.Headers.CacheControl = "no-store";

                await CsvRowWriter.WritePreambleAsync(context.Response.Body, cancellationToken);
                CourseExportPosition? position = null;
                while (true)
                {
                    var chunk = await handler.HandleAsync(query, position, cancellationToken);
                    foreach (var row in chunk.Rows)
                    {
                        await CsvRowWriter.WriteRowAsync(context.Response.Body, row, cancellationToken);
                    }

                    if (chunk.Rows.Count < ExportCoursesQuery.ChunkSize)
                    {
                        break;
                    }

                    position = chunk.NextPosition;
                }
            })
            .WithName("export-courses")
            .WithTags(ServiceNames.Course)
            .Produces(StatusCodes.Status200OK, contentType: "text/csv")
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
}
