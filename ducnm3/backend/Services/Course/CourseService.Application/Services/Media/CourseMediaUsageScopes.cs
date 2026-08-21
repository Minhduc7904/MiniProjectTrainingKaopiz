using MediaService.Contracts.Messaging;

namespace CourseService.Application.Services.Media;

public static class CourseMediaUsageScopes
{
    public static IReadOnlyList<CourseMediaUsageOwnerScope> ForLesson(Guid lessonId) =>
    [
        new(MarkdownMediaUsageOwnerServices.Course, MarkdownMediaUsageOwnerTypes.LessonContent, lessonId),
        new(MarkdownMediaUsageOwnerServices.Course, MarkdownMediaUsageOwnerTypes.LessonAttachment, lessonId),
    ];

    public static IReadOnlyList<CourseMediaUsageOwnerScope> ForCourse(
        Guid courseId,
        IReadOnlyList<Guid> lessonIds)
    {
        var scopes = new List<CourseMediaUsageOwnerScope>
        {
            new(MarkdownMediaUsageOwnerServices.Course, MarkdownMediaUsageOwnerTypes.CourseDescription, courseId),
            new(MarkdownMediaUsageOwnerServices.Course, MarkdownMediaUsageOwnerTypes.CourseThumbnail, courseId),
            new(MarkdownMediaUsageOwnerServices.Course, MarkdownMediaUsageOwnerTypes.CourseGallery, courseId),
        };
        scopes.AddRange(lessonIds.SelectMany(ForLesson));
        return scopes;
    }
}
