using CourseService.Api.Contracts.Courses;
using CourseService.Api.Contracts.Learning;
using CourseService.Application.Services.Content;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.Learning;

namespace CourseService.Api.Mappers;

public static class StudentLearningResponseMapper
{
    public static IReadOnlyList<StudentCourseCatalogResponse> ToCatalogResponses(
        IReadOnlyList<StudentCourseCatalogItem> items,
        IReadOnlyDictionary<Guid, CourseMediaSet> media) =>
        items.Select(item => new StudentCourseCatalogResponse(
            item.CourseId,
            item.Name,
            item.Status,
            item.CreatedAtUtc,
            media.GetValueOrDefault(item.CourseId)?.Thumbnail?.ThumbnailUrl
                ?? media.GetValueOrDefault(item.CourseId)?.Thumbnail?.ContentUrl)).ToArray();

    public static IReadOnlyList<StudentEnrollmentResponse> ToEnrollmentResponses(
        IReadOnlyList<StudentEnrollmentListItem> items,
        IReadOnlyDictionary<Guid, CourseMediaSet> media) =>
        items.Select(item => new StudentEnrollmentResponse(
            item.EnrollmentId,
            item.CourseId,
            item.CourseName,
            item.CourseStatus,
            item.CourseCreatedAtUtc,
            item.EnrolledAtUtc,
            media.GetValueOrDefault(item.CourseId)?.Thumbnail?.ThumbnailUrl
                ?? media.GetValueOrDefault(item.CourseId)?.Thumbnail?.ContentUrl)).ToArray();

    public static StudentEnrollmentDetailResponse ToDetailResponse(
        StudentCourseDetailResult result,
        CourseMediaSet media,
        IMarkdownHtmlRenderer markdownRenderer) =>
        new(
            result.Id,
            result.Name,
            markdownRenderer.Render(result.DescriptionMarkdown),
            result.Status,
            result.CreatedAtUtc,
            ToOptionalMediaResponse(media.Thumbnail),
            media.Gallery.Select(ToMediaResponse).ToArray(),
            result.Lessons.Select(ToLessonPreview).ToArray());

    public static StudentCourseProgressResponse ToProgressResponse(StudentCourseProgressResult result) =>
        new(result.CourseId, result.TotalLessons, result.CompletedLessons, result.ProgressPercent,
            result.NextLesson is null ? null : ToLessonPreview(result.NextLesson));

    private static StudentLessonPreviewResponse ToLessonPreview(StudentLessonPreview lesson) =>
        new(lesson.Id, lesson.Title, lesson.DisplayOrder, lesson.ProgressPercent, lesson.CompletedAtUtc);

    private static CourseMediaResponse? ToOptionalMediaResponse(CourseMediaAsset? media) =>
        media is null ? null : ToMediaResponse(media);

    private static CourseMediaResponse ToMediaResponse(CourseMediaAsset media) =>
        new(media.UsageId, media.MediaId, media.ContentUrl, media.ThumbnailUrl, media.ExpiresAtUtc,
            media.DisplayOrder, media.MediaType, media.ContentType, media.OriginalFileName);
}
