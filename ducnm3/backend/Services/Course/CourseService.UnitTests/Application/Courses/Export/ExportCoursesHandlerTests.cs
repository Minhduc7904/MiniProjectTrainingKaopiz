using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetList;

#pragma warning disable CA1707

namespace CourseService.UnitTests.Application.Courses.Export;

public sealed class ExportCoursesHandlerTests
{
    [Test]
    public async Task HandleAsync_ValidQuery_UsesSingleExportChunkRead()
    {
        var repository = new StubCourseListRepository();
        var handler = new ExportCoursesHandler(repository);
        var query = ExportCoursesQuery.Create(" draft ");
        var position = new CourseExportPosition(
            new DateTime(2026, 8, 19, 7, 30, 0, DateTimeKind.Utc),
            Guid.Parse("11111111-1111-1111-1111-111111111111"));

        using var cancellationTokenSource = new CancellationTokenSource();
        await handler.HandleAsync(query, position, cancellationTokenSource.Token);

        Assert.Multiple(() =>
        {
            Assert.That(repository.ExportCallCount, Is.EqualTo(1));
            Assert.That(repository.LastQuery, Is.SameAs(query));
            Assert.That(repository.LastPosition, Is.EqualTo(position));
            Assert.That(repository.LastCancellationToken, Is.EqualTo(cancellationTokenSource.Token));
            Assert.That(repository.AllCallCount, Is.Zero);
            Assert.That(repository.PagedCallCount, Is.Zero);
        });
    }

    private sealed class StubCourseListRepository : ICourseListRepository
    {
        public int AllCallCount { get; private set; }
        public int PagedCallCount { get; private set; }
        public int ExportCallCount { get; private set; }
        public ExportCoursesQuery? LastQuery { get; private set; }
        public CourseExportPosition? LastPosition { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public Task<IReadOnlyList<CourseListItemRecord>> GetAllAsync(CancellationToken cancellationToken)
        {
            AllCallCount++;
            return Task.FromResult<IReadOnlyList<CourseListItemRecord>>([]);
        }

        public Task<GetCoursesResult> GetPagedAsync(GetCoursesQuery query, CancellationToken cancellationToken)
        {
            PagedCallCount++;
            return Task.FromResult(new GetCoursesResult([], 0, 0));
        }

        public Task<CourseExportChunk> ReadExportChunkAsync(
            ExportCoursesQuery query,
            CourseExportPosition? position,
            CancellationToken cancellationToken)
        {
            ExportCallCount++;
            LastQuery = query;
            LastPosition = position;
            LastCancellationToken = cancellationToken;
            return Task.FromResult(new CourseExportChunk([]));
        }

    }
}
