using CourseService.Application.UseCases.Courses.Export;
using CourseService.Domain.Constants;

#pragma warning disable CA1707

namespace CourseService.UnitTests.Application.Courses.Export;

public sealed class CourseExportChunkTests
{
    [Test]
    public void Constructor_RowCountExceedsChunkSize_ThrowsArgumentOutOfRangeException()
    {
        var rows = Enumerable.Range(0, ExportCoursesQuery.ChunkSize + 1)
            .Select(index => CreateRow(index))
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(() => new CourseExportChunk(rows));
    }

    [Test]
    public void NextPosition_ChunkContainsRows_ReturnsPositionFromFinalRow()
    {
        var finalRow = CreateRow(2);
        var chunk = new CourseExportChunk([CreateRow(1), finalRow]);

        Assert.That(chunk.NextPosition, Is.EqualTo(new CourseExportPosition(finalRow.CreatedAtUtc, finalRow.Id)));
    }

    [Test]
    public void NextPosition_ChunkIsEmpty_ReturnsNull()
    {
        var chunk = new CourseExportChunk([]);

        Assert.That(chunk.NextPosition, Is.Null);
    }

    private static CourseExportRow CreateRow(int index) =>
        new(
            new Guid(index, 0, 0, new byte[8]),
            $"Course {index}",
            CourseStatuses.Published,
            new DateTime(2026, 8, 19, 0, 0, 0, DateTimeKind.Utc).AddMinutes(index));
}
