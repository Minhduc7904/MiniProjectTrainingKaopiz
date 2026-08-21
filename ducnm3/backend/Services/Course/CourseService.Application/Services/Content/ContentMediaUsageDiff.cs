using MediaService.Contracts.Messaging;

namespace CourseService.Application.Services.Content;

public sealed record ContentMediaUsageDiff(
    IReadOnlyList<MarkdownMediaUsageReferenceV1> Added,
    IReadOnlyList<MarkdownMediaUsageReferenceV1> Removed)
{
    public static ContentMediaUsageDiff Create(string? before, string? after)
    {
        var oldReferences = LessonMediaReferenceExtractor.Extract(before);
        var newReferences = LessonMediaReferenceExtractor.Extract(after);
        var oldKeys = oldReferences
            .Select(reference => (reference.MediaId, reference.UsageType))
            .ToHashSet();
        var newKeys = newReferences
            .Select(reference => (reference.MediaId, reference.UsageType))
            .ToHashSet();

        return new ContentMediaUsageDiff(
            newReferences.Where(reference => !oldKeys.Contains((reference.MediaId, reference.UsageType))).ToArray(),
            oldReferences.Where(reference => !newKeys.Contains((reference.MediaId, reference.UsageType))).ToArray());
    }
}
