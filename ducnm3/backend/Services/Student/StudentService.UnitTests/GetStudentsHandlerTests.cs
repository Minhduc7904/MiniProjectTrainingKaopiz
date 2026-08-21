using StudentService.Application.Repositories;
using StudentService.Application.UseCases.Students.GetList;

namespace StudentService.UnitTests;

public sealed class GetStudentsHandlerTests
{
    [Test]
    public async Task ValidQueryReturnsRepositoryPage()
    {
        var expected = new StudentListPage(
            [
                new StudentListRecord(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "student@example.com",
                    "Student",
                    "ACTIVE",
                    new DateTime(2026, 8, 13, 1, 2, 3, DateTimeKind.Utc)),
            ],
            1,
            1);
        var repository = new SpyStudentListRepository(expected);
        var handler = new GetStudentsHandler(repository);
        var query = GetStudentsQuery.Create(
            "ACTIVE",
            "createdAt",
            "desc",
            1,
            20);
        using var cancellation = new CancellationTokenSource();

        var actual = await handler.HandleAsync(query, cancellation.Token);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Items, Is.SameAs(expected.Items));
            Assert.That(actual.TotalItems, Is.EqualTo(expected.TotalItems));
            Assert.That(repository.Query, Is.SameAs(query));
            Assert.That(repository.CancellationToken, Is.EqualTo(cancellation.Token));
            Assert.That(repository.CallCount, Is.EqualTo(1));
        });
    }

    private sealed class SpyStudentListRepository(
        StudentListPage result) : IStudentListRepository
    {
        public int CallCount { get; private set; }

        public GetStudentsQuery? Query { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<StudentListPage> GetListAsync(
            GetStudentsQuery query,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Query = query;
            CancellationToken = cancellationToken;
            return Task.FromResult(result);
        }
    }
}
