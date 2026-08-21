using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Learning;

namespace CourseService.UnitTests.Application.Learning;

public sealed class StudentLearningHandlersTests
{
    private static readonly Guid StudentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public void EnrollmentQueryRejectsInvalidOffsetPagination()
    {
        var exception = Assert.Throws<CourseApplicationException>(
            () => GetStudentEnrollmentsQuery.Create(0, 51));

        Assert.That(exception?.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CatalogHandlerReturnsOnlyCoursesProvidedByRepository()
    {
        var expected = new StudentCourseCatalogResult([
            new StudentCourseCatalogItem(CourseId, "Backend Fundamentals", "PUBLISHED", DateTime.UnixEpoch),
        ], 1, 1);
        var handler = new GetStudentCourseCatalogHandler(new StubRepository { Catalog = expected });

        var result = await handler.HandleAsync(StudentId, GetStudentCourseCatalogQuery.Create(1, 12), TestContext.CurrentContext.CancellationToken);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task ProgressHandlerRejectsStudentWithoutEnrollment()
    {
        var handler = new GetMyCourseProgressHandler(new StubRepository { IsEnrolled = false });

        var exception = Assert.ThrowsAsync<CourseApplicationException>(
            () => handler.HandleAsync(CourseId, StudentId, TestContext.CurrentContext.CancellationToken));

        Assert.That(exception?.StatusCode, Is.EqualTo(403));
        Assert.That(exception?.ErrorCode, Is.EqualTo("STUDENT_NOT_ENROLLED"));
    }

    [Test]
    public async Task DetailHandlerRejectsStudentWithoutEnrollmentBeforeReadingCourse()
    {
        var handler = new GetStudentEnrollmentDetailHandler(new StubRepository { IsEnrolled = false });

        var exception = Assert.ThrowsAsync<CourseApplicationException>(
            () => handler.HandleAsync(CourseId, StudentId, TestContext.CurrentContext.CancellationToken));

        Assert.That(exception?.ErrorCode, Is.EqualTo("STUDENT_NOT_ENROLLED"));
    }

    [Test]
    public async Task ProgressHandlerReturnsStudentSpecificProgress()
    {
        var expected = new StudentCourseProgressResult(CourseId, 4, 2, 50, new StudentLessonPreview(
            Guid.Parse("22222222-2222-2222-2222-222222222222"), "Tiếp theo", 3));
        var handler = new GetMyCourseProgressHandler(new StubRepository { Progress = expected });

        var result = await handler.HandleAsync(CourseId, StudentId, TestContext.CurrentContext.CancellationToken);

        Assert.That(result, Is.EqualTo(expected));
    }

    private sealed class StubRepository : IStudentLearningRepository
    {
        public bool IsEnrolled { get; init; } = true;
        public StudentCourseProgressResult? Progress { get; init; }
        public StudentCourseCatalogResult? Catalog { get; init; }

        public Task<StudentEnrollmentsResult> GetEnrollmentsAsync(Guid studentId, GetStudentEnrollmentsQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(new StudentEnrollmentsResult([], 0, 0));

        public Task<bool> IsEnrolledAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) =>
            Task.FromResult(IsEnrolled);

        public Task<StudentCourseDetailResult?> GetDetailAsync(Guid courseId, CancellationToken cancellationToken) =>
            Task.FromResult<StudentCourseDetailResult?>(null);

        public Task<StudentCourseProgressResult?> GetProgressAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) =>
            Task.FromResult(Progress);

        public Task<StudentCourseCatalogResult> GetCatalogAsync(Guid studentId, GetStudentCourseCatalogQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(Catalog ?? new StudentCourseCatalogResult([], 0, 0));
    }
}
