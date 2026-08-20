namespace CourseService.Application.Repositories;

public interface ILessonCommandRepository
{
    Task<LessonCreateRecord?> CreateAsync(
        Guid courseId,
        string title,
        string? contentMarkdown,
        uint? displayOrder,
        CancellationToken cancellationToken);
}

public sealed record LessonCreateRecord(
    Guid Id,
    Guid CourseId,
    string Title,
    string? ContentMarkdown,
    uint DisplayOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
