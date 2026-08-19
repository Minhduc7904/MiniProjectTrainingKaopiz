using System.Text;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetList;
using CourseService.Domain.Constants;

namespace CourseService.UnitTests.Application.Courses.Export;

public sealed class BufferedCourseExportHandlerTests
{
    [Test]
    public async Task HandleAsyncValidQueryMaterializesRowsAndWritesCsvBytes()
    {
        var repository = new StubCourseListRepository();
        var handler = new BufferedCourseExportHandler(repository);

        var bytes = await handler.HandleAsync(
            ExportCoursesQuery.Create("published"),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(repository.BufferedExportCallCount, Is.EqualTo(1));
            Assert.That(repository.LastQuery?.Status, Is.EqualTo(CourseStatuses.Published));
            Assert.That(bytes.Take(3), Is.EqualTo(Encoding.UTF8.GetPreamble()));
            Assert.That(Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3), Does.Contain(
                "id,name,status,createdAtUtc\r\n"));
        });
    }

    private sealed class StubCourseListRepository : ICourseListRepository
    {
        public int BufferedExportCallCount { get; private set; }
        public ExportCoursesQuery? LastQuery { get; private set; }

        public Task<IReadOnlyList<CourseListItemRecord>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CourseListItemRecord>>([]);

        public Task<GetCoursesResult> GetPagedAsync(
            GetCoursesQuery query,
            CancellationToken cancellationToken) =>
            Task.FromResult(new GetCoursesResult([], 0, 0));

        public Task<CourseExportChunk> ReadExportChunkAsync(
            ExportCoursesQuery query,
            CourseExportPosition? position,
            CancellationToken cancellationToken) =>
            Task.FromResult(new CourseExportChunk([]));

        public Task<IReadOnlyList<CourseExportRow>> ReadAllExportRowsAsync(
            ExportCoursesQuery query,
            CancellationToken cancellationToken)
        {
            BufferedExportCallCount++;
            LastQuery = query;
            return Task.FromResult<IReadOnlyList<CourseExportRow>>(
            [
                new CourseExportRow(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Backend Fundamentals",
                    CourseStatuses.Published,
                    new DateTime(2026, 8, 19, 7, 30, 0, DateTimeKind.Utc)),
            ]);
        }
    }
}
