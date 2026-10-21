using BuildingBlocks.Contracts.Api;

namespace CourseService.UnitTests;

public sealed class ApiRoutesTests
{
    private static readonly Guid CourseId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid LessonId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Test]
    public void LearningPathsUseServiceAndGatewayShapes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                ApiRoutes.Courses.EnrollmentsServicePath(CourseId),
                Is.EqualTo("api/courses/11111111-1111-1111-1111-111111111111/enrollments"));
            Assert.That(
                ApiRoutes.Courses.EnrollmentsPublicPath(CourseId),
                Is.EqualTo("/course/api/courses/11111111-1111-1111-1111-111111111111/enrollments"));
            Assert.That(
                ApiRoutes.Courses.CompleteLessonProgressServicePath(CourseId, LessonId),
                Is.EqualTo("api/courses/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222/progress/complete"));
            Assert.That(
                ApiRoutes.Courses.CompleteLessonProgressPublicPath(CourseId, LessonId),
                Is.EqualTo("/course/api/courses/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222/progress/complete"));
            Assert.That(ApiRoutes.Courses.StudentEnrollmentsServicePath(), Is.EqualTo("api/student/enrollments"));
            Assert.That(ApiRoutes.Courses.StudentEnrollmentsPublicPath(), Is.EqualTo("/course/api/student/enrollments"));
            Assert.That(ApiRoutes.Courses.StudentCourseCatalogPublicPath(), Is.EqualTo("/course/api/student/courses"));
            Assert.That(
                ApiRoutes.Courses.StudentEnrollmentDetailPublicPath(CourseId),
                Is.EqualTo("/course/api/student/enrollments/11111111-1111-1111-1111-111111111111"));
            Assert.That(
                ApiRoutes.Courses.MyProgressPublicPath(CourseId),
                Is.EqualTo("/course/api/courses/11111111-1111-1111-1111-111111111111/my-progress"));
        });
    }

    [Test]
    public void QuizPathsUseServiceAndGatewayShapes()
    {
        var quizId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var attemptId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        Assert.Multiple(() =>
        {
            Assert.That(
                ApiRoutes.Courses.QuizByIdServicePath(CourseId, LessonId, quizId),
                Is.EqualTo("api/courses/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222/quizzes/33333333-3333-3333-3333-333333333333"));
            Assert.That(
                ApiRoutes.Courses.QuizByIdPublicPath(CourseId, LessonId, quizId),
                Is.EqualTo("/course/api/courses/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222/quizzes/33333333-3333-3333-3333-333333333333"));
            Assert.That(
                ApiRoutes.Courses.StudentQuizAttemptByIdPublicPath(CourseId, LessonId, quizId, attemptId),
                Is.EqualTo("/course/api/student/enrollments/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222/quizzes/33333333-3333-3333-3333-333333333333/attempts/44444444-4444-4444-4444-444444444444"));
        });
    }

    [Test]
    public void LessonAiPathsUseServiceAndGatewayShapes()
    {
        var jobId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        const string lesson = "api/courses/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222";

        Assert.Multiple(() =>
        {
            Assert.That(ApiRoutes.Courses.LessonAiDocumentsServicePath(CourseId, LessonId), Is.EqualTo($"{lesson}/ai-documents"));
            Assert.That(ApiRoutes.Courses.LessonAiDocumentsPublicPath(CourseId, LessonId), Is.EqualTo($"/course/{lesson}/ai-documents"));
            Assert.That(ApiRoutes.Courses.LessonAiDocumentsSyncServicePath(CourseId, LessonId), Is.EqualTo($"{lesson}/ai-documents/sync"));
            Assert.That(ApiRoutes.Courses.LessonAiJobsServicePath(CourseId, LessonId), Is.EqualTo($"{lesson}/ai-jobs"));
            Assert.That(ApiRoutes.Courses.LessonAiJobByIdServicePath(CourseId, LessonId, jobId), Is.EqualTo($"{lesson}/ai-jobs/55555555-5555-5555-5555-555555555555"));
            Assert.That(ApiRoutes.Courses.LessonAiJobByIdPublicPath(CourseId, LessonId, jobId), Is.EqualTo($"/course/{lesson}/ai-jobs/55555555-5555-5555-5555-555555555555"));
        });
    }
}
