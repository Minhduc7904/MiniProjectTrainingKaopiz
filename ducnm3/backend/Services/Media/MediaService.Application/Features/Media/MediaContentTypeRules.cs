using System.Text.RegularExpressions;
using MediaService.Application.Abstractions.Storage;

namespace MediaService.Application.Features.Media;

public static partial class MediaContentTypeRules
{
    private static readonly HashSet<string> DocumentContentTypes =
    [
        "application/msword",
        "application/pdf",
        "application/rtf",
        "application/vnd.ms-excel",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/csv",
        "text/markdown",
        "text/plain"
    ];

    public static string Normalize(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw MediaErrors.InvalidMedia("Content type is required.");
        }

        var normalized = contentType.Split(';', 2)[0].Trim().ToLowerInvariant();
        if (!ContentTypePattern().IsMatch(normalized))
        {
            throw MediaErrors.InvalidMedia("Content type is invalid.");
        }

        return normalized;
    }

    public static bool Matches(
        StorageMediaCategory category,
        string normalizedContentType) =>
        category switch
        {
            StorageMediaCategory.Image =>
                normalizedContentType.StartsWith("image/", StringComparison.Ordinal),
            StorageMediaCategory.Video =>
                normalizedContentType.StartsWith("video/", StringComparison.Ordinal),
            StorageMediaCategory.Document =>
                DocumentContentTypes.Contains(normalizedContentType),
            StorageMediaCategory.Audio =>
                normalizedContentType.StartsWith("audio/", StringComparison.Ordinal),
            StorageMediaCategory.Other => !IsKnownCategory(normalizedContentType),
            _ => false
        };

    private static bool IsKnownCategory(string contentType) =>
        contentType.StartsWith("image/", StringComparison.Ordinal) ||
        contentType.StartsWith("video/", StringComparison.Ordinal) ||
        contentType.StartsWith("audio/", StringComparison.Ordinal) ||
        DocumentContentTypes.Contains(contentType);

    [GeneratedRegex(
        "^[a-z0-9][a-z0-9!#$&^_.+-]*/[a-z0-9][a-z0-9!#$&^_.+-]*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex ContentTypePattern();
}
