namespace CourseService.Application.Services.Media;

public interface ICourseMediaReader
{
    Task<CourseMediaSet> GetAsync(Guid courseId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, CourseMediaSet>> GetManyAsync(
        IReadOnlyList<Guid> courseIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseMediaAsset>> GetLessonAttachmentsAsync(
        Guid lessonId,
        CancellationToken cancellationToken);
}

public sealed record CourseMediaSet(
    CourseMediaAsset? Thumbnail,
    IReadOnlyList<CourseMediaAsset> Gallery);

public sealed record CourseMediaAsset(
    Guid UsageId,
    Guid MediaId,
    Guid OwnerId,
    string ContentUrl,
    string? ThumbnailUrl,
    DateTime? ExpiresAtUtc,
    uint DisplayOrder,
    string MediaType,
    string ContentType,
    string OriginalFileName);
