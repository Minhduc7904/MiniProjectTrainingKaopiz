using CourseService.Api.Contracts.Courses;

namespace CourseService.Api.Contracts.Learning;

public sealed record StudentEnrollmentResponse(
    Guid EnrollmentId,
    Guid CourseId,
    string Name,
    string Status,
    DateTime CreatedAtUtc,
    DateTime EnrolledAtUtc,
    string? ThumbnailUrl);

public sealed record StudentCourseCatalogResponse(
    Guid CourseId,
    string Name,
    string Status,
    DateTime CreatedAtUtc,
    string? ThumbnailUrl);

public sealed record StudentLessonPreviewResponse(Guid Id, string Title, uint DisplayOrder, decimal ProgressPercent, DateTime? CompletedAtUtc);

public sealed record StudentEnrollmentDetailResponse(
    Guid Id,
    string Name,
    string? DescriptionHtml,
    string Status,
    DateTime CreatedAtUtc,
    CourseMediaResponse? Thumbnail,
    IReadOnlyList<CourseMediaResponse> Gallery,
    IReadOnlyList<StudentLessonPreviewResponse> Lessons);

public sealed record StudentCourseProgressResponse(
    Guid CourseId,
    int TotalLessons,
    int CompletedLessons,
    decimal ProgressPercent,
    StudentLessonPreviewResponse? NextLesson);
