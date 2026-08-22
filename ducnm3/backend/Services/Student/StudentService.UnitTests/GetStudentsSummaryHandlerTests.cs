using StudentService.Application.Repositories;
using StudentService.Application.UseCases.Students.GetSummary;

namespace StudentService.UnitTests;

public sealed class GetStudentsSummaryHandlerTests
{
    [Test]
    public async Task HandleAsync_RepositoryReturnsCount_MapsTotalStudents()
    {
        var handler = new GetStudentsSummaryHandler(new Repository(42));

        var result = await handler.HandleAsync(TestContext.CurrentContext.CancellationToken);

        Assert.That(result.TotalStudents, Is.EqualTo(42));
    }

    private sealed class Repository(long count) : IStudentSummaryRepository
    {
        public Task<long> CountAsync(CancellationToken cancellationToken) => Task.FromResult(count);
    }
}
