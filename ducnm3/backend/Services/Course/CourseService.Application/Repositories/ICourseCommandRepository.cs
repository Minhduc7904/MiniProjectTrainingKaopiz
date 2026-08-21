namespace CourseService.Application.Repositories;

public interface ICourseCommandRepository
{
    Task<CourseCommandRecord?> GetAsync(Guid courseId, CancellationToken cancellationToken);
    Task<CourseCommandRecord> CreateAsync(string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken);
    Task<CourseCommandRecord?> UpdateAsync(Guid courseId, string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> GetLessonIdsAsync(Guid courseId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid courseId, CancellationToken cancellationToken);
}

public sealed record CourseCommandRecord(Guid Id, string Name, string? DescriptionMarkdown, string Status, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
