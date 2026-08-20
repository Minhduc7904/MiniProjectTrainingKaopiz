using System.Text.RegularExpressions;
using BuildingBlocks.Contracts.Api;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.Services.Content;

public sealed partial class LessonMediaReferenceExtractor
{
    public static IReadOnlyList<NotificationMediaUsageReferenceV1> Extract(string? contentMarkdown)
    {
        if (string.IsNullOrWhiteSpace(contentMarkdown)) return [];
        var references = new List<NotificationMediaUsageReferenceV1>();
        var seen = new HashSet<(Guid, string)>();
        foreach (Match match in MarkdownLinkRegex().Matches(contentMarkdown))
        {
            if (!TryGetMediaId(match.Groups["url"].Value, out var mediaId)) continue;
            var usageType = match.Groups["embed"].Success
                ? NotificationMediaUsageTypes.Embed
                : NotificationMediaUsageTypes.Attachment;
            if (seen.Add((mediaId, usageType)))
            {
                references.Add(new NotificationMediaUsageReferenceV1(mediaId, usageType, checked((uint)references.Count)));
            }
        }
        return references;
    }

    private static bool TryGetMediaId(string value, out Guid mediaId)
    {
        mediaId = Guid.Empty;
        const string prefix = "/media/api/media/";
        const string suffix = "/content";
        if (!value.StartsWith(prefix, StringComparison.Ordinal) || !value.EndsWith(suffix, StringComparison.Ordinal)) return false;
        var id = value[prefix.Length..^suffix.Length];
        return Guid.TryParse(id, out mediaId) && mediaId != Guid.Empty && value == ApiRoutes.Media.ContentPublicPath(mediaId);
    }

    [GeneratedRegex(@"(?<embed>!)?\[[^\]]*\]\((?<url>[^\s)]+)(?:\s+[^)]*)?\)", RegexOptions.CultureInvariant)]
    private static partial Regex MarkdownLinkRegex();
}
