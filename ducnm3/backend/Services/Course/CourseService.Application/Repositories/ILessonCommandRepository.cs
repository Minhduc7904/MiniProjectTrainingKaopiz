namespace CourseService.Application.Repositories;

public interface ILessonCommandRepository
{
    Task<LessonCreateRecord?> CreateAsync(
        Guid courseId,
        string title,
        string? contentMarkdown,
        uint? displayOrder,
        CancellationToken cancellationToken);

    Task<LessonCreateRecord?> GetAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken);

    Task<LessonCreateRecord?> UpdateAsync(Guid courseId, Guid lessonId, string title, string? contentMarkdown, CancellationToken cancellationToken);

    Task<bool> ReorderAsync(Guid courseId, IReadOnlyList<Guid> lessonIds, CancellationToken cancellationToken);
}

public sealed record LessonCreateRecord(
    Guid Id,
    Guid CourseId,
    string Title,
    string? ContentMarkdown,
    uint DisplayOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
