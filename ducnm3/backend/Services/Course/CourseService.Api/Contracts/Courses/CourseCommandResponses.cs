namespace CourseService.Api.Contracts.Courses;

public sealed record CourseCommandResponse(Guid Id, string Name, string? DescriptionMarkdown, string Status, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public sealed record LessonCommandResponse(Guid Id, Guid CourseId, string Title, string? ContentMarkdown, string? ContentHtml, uint DisplayOrder, DateTime CreatedAtUtc, DateTime UpdatedAtUtc, IReadOnlyList<CourseMediaResponse> Attachments);
