// File: backend/Services/Course/CourseService.Api/Endpoints/Courses/Export/ExportCoursesEndpoint.cs
// Mục đích: Stream CSV Course theo chunk keyset trực tiếp vào HTTP response.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.UseCases.Courses.Export;

namespace CourseService.Api.Endpoints.Courses.Export;

public static class ExportCoursesEndpoint
{
    // Trình duyệt dùng header này để tải file thay vì hiển thị body CSV như text trên tab hiện tại.
    private const string ContentDisposition = "attachment; filename=\"courses.csv\"";

    public static RouteHandlerBuilder MapExportCourses(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.Export, async (
                string? status,
            int? limit,
                HttpContext context,
                ExportCoursesHandler handler,
                CancellationToken cancellationToken) =>
            {
                // Parse/validate status và limit trước khi bắt đầu ghi response. Nếu query không hợp lệ,
                // endpoint trả lỗi bình thường vì chưa có byte CSV nào được gửi cho client.
                var query = ExportCoursesQuery.Create(status, limit);

                // Khai báo response streaming. Không tạo byte[] hoặc MemoryStream chứa toàn bộ file ở endpoint này.
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/csv; charset=utf-8";
                context.Response.Headers.ContentDisposition = ContentDisposition;
                context.Response.Headers.CacheControl = "no-store";

                // Ghi BOM UTF-8 và header đúng một lần trực tiếp vào HTTP response stream.
                // Client có thể bắt đầu tải/hiển thị phần đầu file ngay khi các chunk sau còn đang được đọc.
                await CsvRowWriter.WritePreambleAsync(context.Response.Body, cancellationToken);

                // position là cursor nội bộ của keyset pagination, ban đầu null nghĩa là đọc từ Course mới nhất.
                CourseExportPosition? position = null;
                while (true)
                {
                    // Mỗi vòng chỉ yêu cầu Infrastructure đọc một chunk (tối đa 500 rows), không load toàn bộ Course.
                    var chunk = await handler.HandleAsync(query, position, cancellationToken);
                    foreach (var row in chunk.Rows)
                    {
                        // Row được serialize và ghi thẳng xuống network stream, sau đó có thể được GC.
                        await CsvRowWriter.WriteRowAsync(context.Response.Body, row, cancellationToken);
                    }

                    // Một chunk ngắn hơn ChunkSize là trang cuối. Điều kiện này cũng xử lý kết quả rỗng.
                    if (chunk.Rows.Count < ExportCoursesQuery.ChunkSize)
                    {
                        break;
                    }

                    // Cursor lấy từ row cuối: vòng sau dùng predicate keyset để không lặp lại các row đã xuất.
                    position = chunk.NextPosition;
                }
            })
            .WithName("export-courses")
            .WithTags(ServiceNames.Course)
            .Produces(StatusCodes.Status200OK, contentType: "text/csv")
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
}
