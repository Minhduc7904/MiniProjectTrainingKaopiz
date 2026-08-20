namespace CourseService.Application.UseCases.Courses.GetDetails;

public sealed record CourseDetailsResult(
    Guid Id,
    string Name,
    string? DescriptionMarkdown,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<LessonDetailsResult> Lessons);

public sealed record LessonDetailsResult(
    Guid Id,
    string Title,
    string? ContentMarkdown,
    uint DisplayOrder,
    IReadOnlyList<LessonProgressDetailsResult> Progresses);

public sealed record LessonProgressDetailsResult(
    Guid StudentId,
    decimal ProgressPercent,
    DateTime? CompletedAtUtc,
    DateTime UpdatedAtUtc);
