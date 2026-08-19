using System.Text;
using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Courses.Export;

public sealed class BufferedCourseExportHandler(ICourseListRepository courseListRepository)
{
    public async Task<byte[]> HandleAsync(
        ExportCoursesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var rows = await courseListRepository.ReadAllExportRowsAsync(query, cancellationToken);
        await using var stream = new MemoryStream();
        await CsvRowWriter.WritePreambleAsync(stream, cancellationToken);
        foreach (var row in rows)
        {
            await CsvRowWriter.WriteRowAsync(stream, row, cancellationToken);
        }

        return stream.ToArray();
    }
}
