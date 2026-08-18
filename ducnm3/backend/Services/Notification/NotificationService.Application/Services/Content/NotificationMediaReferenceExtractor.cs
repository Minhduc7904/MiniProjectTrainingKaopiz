// File: backend/Services/Notification/NotificationService.Application/Services/Content/NotificationMediaReferenceExtractor.cs
// Mục đích: Quét Markdown để lấy MediaId và vị trí embed, phục vụ đăng ký Media Usage sau khi gửi thành công.

using System.Text.RegularExpressions;
using BuildingBlocks.Contracts.Api;
using MediaService.Contracts.Messaging;
using NotificationService.Application.Common.Errors;

namespace NotificationService.Application.Services.Content;

public sealed partial class NotificationMediaReferenceExtractor
{
    private readonly Regex markdownLinkRegex = MarkdownLinkRegex();

    public IReadOnlyList<NotificationMediaUsageReferenceV1> Extract(string bodyMarkdown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bodyMarkdown);

        var references = new List<NotificationMediaUsageReferenceV1>();
        var seen = new HashSet<(Guid MediaId, string UsageType)>();
        foreach (Match match in markdownLinkRegex.Matches(bodyMarkdown))
        {
            var url = match.Groups["url"].Value;
            if (!TryGetMediaId(url, out var mediaId))
            {
                throw NotificationErrors.Validation(
                    "Markdown media links must use a Media Service contentUrl.");
            }

            var usageType = match.Groups["embed"].Success
                ? NotificationMediaUsageTypes.Embed
                : NotificationMediaUsageTypes.Attachment;
            if (seen.Add((mediaId, usageType)))
            {
                references.Add(new NotificationMediaUsageReferenceV1(
                    mediaId,
                    usageType,
                    checked((uint)references.Count)));
            }
        }

        return references;
    }

    private static bool TryGetMediaId(string value, out Guid mediaId)
    {
        mediaId = Guid.Empty;
        const string prefix = "/media/api/media/";
        const string suffix = "/content";
        if (!value.StartsWith(prefix, StringComparison.Ordinal) ||
            !value.EndsWith(suffix, StringComparison.Ordinal))
        {
            return false;
        }

        var id = value[prefix.Length..^suffix.Length];
        return Guid.TryParse(id, out mediaId) &&
               mediaId != Guid.Empty &&
               string.Equals(
                   value,
                   ApiRoutes.Media.ContentPublicPath(mediaId),
                   StringComparison.Ordinal);
    }

    [GeneratedRegex(@"(?<embed>!)?\[[^\]]*\]\((?<url>[^\s)]+)(?:\s+[^)]*)?\)", RegexOptions.CultureInvariant)]
    private static partial Regex MarkdownLinkRegex();
}
