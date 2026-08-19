using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetList;
using CourseService.Domain.Constants;

#pragma warning disable CA1707

namespace CourseService.UnitTests.Application.Courses.GetList;

public sealed class GetCoursesHandlerTests
{
    [Test]
    public async Task HandleAsync_ValidOffsetQuery_UsesPagedRepositoryOnly()
    {
        var repository = new StubCourseListRepository();
        var handler = new GetCoursesHandler(repository);
        var query = GetCoursesQuery.Create(" published ", null, null, null, null);

        await handler.HandleAsync(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(query.Status, Is.EqualTo(CourseStatuses.Published));
            Assert.That(query.SortBy, Is.EqualTo(CourseSortField.CreatedAt));
            Assert.That(query.Descending, Is.True);
            Assert.That(query.Page, Is.EqualTo(1));
            Assert.That(query.PageSize, Is.EqualTo(20));
            Assert.That(repository.PagedCallCount, Is.EqualTo(1));
            Assert.That(repository.AllCallCount, Is.Zero);
        });
    }

    private sealed class StubCourseListRepository : ICourseListRepository
    {
        public int AllCallCount { get; private set; }
        public int PagedCallCount { get; private set; }

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


        public Task<CourseExportChunk> ReadExportChunkAsync(ExportCoursesQuery query, CourseExportPosition? position, CancellationToken cancellationToken) =>
            Task.FromResult(new CourseExportChunk([]));

        public Task<IReadOnlyList<CourseExportRow>> ReadAllExportRowsAsync(
            ExportCoursesQuery query,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CourseExportRow>>([]);
    }
}
