using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.GetSummary;

namespace CourseService.UnitTests;

public sealed class GetCoursesSummaryHandlerTests
{
    [Test]
    public async Task HandleAsync_RepositoryReturnsCounts_MapsCourseAndLessonTotals()
    {
        var handler = new GetCoursesSummaryHandler(new Repository(12, 37));

        var result = await handler.HandleAsync(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCourses, Is.EqualTo(12));
            Assert.That(result.TotalLessons, Is.EqualTo(37));
        });
    }

    private sealed class Repository(long courses, long lessons) : ICourseSummaryRepository
    {
        public Task<(long TotalCourses, long TotalLessons)> CountAsync(CancellationToken cancellationToken) =>
            Task.FromResult((courses, lessons));
    }
}
