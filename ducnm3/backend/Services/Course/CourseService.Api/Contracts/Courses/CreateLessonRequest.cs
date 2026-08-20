namespace CourseService.Api.Contracts.Courses;

public sealed record CreateLessonRequest(
    string Title,
    string? ContentMarkdown,
    uint? DisplayOrder);

public sealed record CreateLessonResponse(
    Guid Id,
    Guid CourseId,
    string Title,
    string? ContentMarkdown,
    uint DisplayOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
