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
        });
    }
}
