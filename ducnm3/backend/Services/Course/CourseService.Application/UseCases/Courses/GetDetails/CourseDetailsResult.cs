namespace CourseService.Application.UseCases.Courses.GetDetails;

public sealed record CourseDetailsResult(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<LessonDetailsResult> Lessons);

public sealed record LessonDetailsResult(
    Guid Id,
    string Title,
    uint DisplayOrder,
    IReadOnlyList<LessonProgressDetailsResult> Progresses);

public sealed record LessonProgressDetailsResult(
    Guid StudentId,
    decimal ProgressPercent,
    DateTime? CompletedAtUtc,
    DateTime UpdatedAtUtc);
