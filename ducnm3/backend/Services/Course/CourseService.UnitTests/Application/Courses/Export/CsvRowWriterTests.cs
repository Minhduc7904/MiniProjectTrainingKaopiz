using System.Text;
using CourseService.Application.UseCases.Courses.Export;

#pragma warning disable CA1707

namespace CourseService.UnitTests.Application.Courses.Export;

public sealed class CsvRowWriterTests
{
    [Test]
    public async Task WritePreambleAndRowAsync_FieldsContainCsvSpecialCharacters_WritesBomHeaderAndEscapedRow()
    {
        await using var stream = new MemoryStream();
        var row = new CourseExportRow(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Backend, \"Fundamentals\"\r\nPart 1",
            "PUBLISHED",
            new DateTime(2026, 8, 19, 7, 30, 0, DateTimeKind.Utc));

        await CsvRowWriter.WritePreambleAsync(stream, CancellationToken.None);
        await CsvRowWriter.WriteRowAsync(stream, row, CancellationToken.None);

        var bytes = stream.ToArray();
        var content = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        Assert.Multiple(() =>
        {
            Assert.That(bytes.Take(3), Is.EqualTo(Encoding.UTF8.GetPreamble()));
            Assert.That(content, Is.EqualTo(
                "id,name,status,createdAtUtc\r\n" +
                "11111111-1111-1111-1111-111111111111,\"Backend, \"\"Fundamentals\"\"\r\nPart 1\",PUBLISHED,2026-08-19T07:30:00.0000000Z\r\n"));
        });
    }
}
