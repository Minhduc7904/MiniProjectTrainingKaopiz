namespace CourseService.Api.Contracts.Courses;

public sealed record CourseMediaResponse(
    Guid UsageId,
    Guid MediaId,
    string ContentUrl,
    string? ThumbnailUrl,
    DateTime? ExpiresAtUtc,
    uint DisplayOrder);

public sealed record CourseDetailsResponse(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedAtUtc,
    CourseMediaResponse? Thumbnail,
    IReadOnlyList<CourseMediaResponse> Gallery,
    IReadOnlyList<LessonDetailsResponse> Lessons);

public sealed record LessonDetailsResponse(
    Guid Id,
    string Title,
    uint DisplayOrder,
    IReadOnlyList<LessonProgressResponse> Progresses);

public sealed record LessonProgressResponse(
    Guid StudentId,
    decimal ProgressPercent,
    DateTime? CompletedAtUtc,
    DateTime UpdatedAtUtc);
