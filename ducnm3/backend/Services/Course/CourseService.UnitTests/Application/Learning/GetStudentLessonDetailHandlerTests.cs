using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Learning;

namespace CourseService.UnitTests.Application.Learning;

public sealed class GetStudentLessonDetailHandlerTests
{
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LessonId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid StudentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Test]
    public async Task HandleAsync_ReturnsLessonOnlyAfterEnrollmentVerification()
    {
        var lesson = new LessonCreateRecord(LessonId, CourseId, "Bắt đầu", "# Nội dung", 1, DateTime.UnixEpoch, DateTime.UnixEpoch);
        var learning = new LearningRepository { IsEnrolled = true };
        var handler = new GetStudentLessonDetailHandler(learning, new LessonRepository { Lesson = lesson });

        var result = await handler.HandleAsync(CourseId, LessonId, StudentId, TestContext.CurrentContext.CancellationToken);

        Assert.That(result, Is.EqualTo(lesson));
    }

    [Test]
    public void HandleAsync_RejectsStudentWithoutEnrollmentBeforeReadingLesson()
    {
        var lessons = new LessonRepository();
        var handler = new GetStudentLessonDetailHandler(new LearningRepository { IsEnrolled = false }, lessons);

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() =>
            handler.HandleAsync(CourseId, LessonId, StudentId, TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(exception?.ErrorCode, Is.EqualTo("STUDENT_NOT_ENROLLED"));
            Assert.That(lessons.ReadRequested, Is.False);
        });
    }

    [Test]
    public void HandleAsync_ReturnsNotFoundWhenLessonDoesNotBelongToCourse()
    {
        var handler = new GetStudentLessonDetailHandler(
            new LearningRepository { IsEnrolled = true },
            new LessonRepository());

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() =>
            handler.HandleAsync(CourseId, LessonId, StudentId, TestContext.CurrentContext.CancellationToken));

        Assert.That(exception?.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonNotFound));
    }

    private sealed class LearningRepository : IStudentLearningRepository
    {
        public bool IsEnrolled { get; init; }
        public Task<bool> IsEnrolledAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) => Task.FromResult(IsEnrolled);
        public Task<StudentEnrollmentsResult> GetEnrollmentsAsync(Guid studentId, GetStudentEnrollmentsQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<StudentCourseCatalogResult> GetCatalogAsync(Guid studentId, GetStudentCourseCatalogQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<StudentCourseDetailResult?> GetDetailAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<StudentCourseProgressResult?> GetProgressAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class LessonRepository : ILessonCommandRepository
    {
        public LessonCreateRecord? Lesson { get; init; }
        public bool ReadRequested { get; private set; }
        public Task<LessonCreateRecord?> GetAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken)
        {
            ReadRequested = true;
            return Task.FromResult(Lesson);
        }
        public Task<LessonCreateRecord?> CreateAsync(Guid courseId, string title, string? contentMarkdown, uint? displayOrder, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<LessonCreateRecord?> UpdateAsync(Guid courseId, Guid lessonId, string title, string? contentMarkdown, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> ReorderAsync(Guid courseId, IReadOnlyList<Guid> lessonIds, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
