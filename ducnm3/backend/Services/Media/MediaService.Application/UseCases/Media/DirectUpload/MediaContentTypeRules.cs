// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/MediaContentTypeRules.cs
// Mục đích: Khai báo quy tắc content type được hỗ trợ cho upload trực tiếp theo từng nhóm media.

using System.Text.RegularExpressions;
using MediaService.Application.Common.Errors;
using MediaService.Application.Services.Storage;
using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.Media.DirectUpload;

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

    public static StorageMediaCategory InferCategory(string contentType)
    {
        var normalized = Normalize(contentType);
        if (normalized.StartsWith("image/", StringComparison.Ordinal))
            return StorageMediaCategory.Image;
        if (normalized.StartsWith("video/", StringComparison.Ordinal))
            return StorageMediaCategory.Video;
        if (normalized.StartsWith("audio/", StringComparison.Ordinal))
            return StorageMediaCategory.Audio;
        if (DocumentContentTypes.Contains(normalized))
            return StorageMediaCategory.Document;
        return StorageMediaCategory.Other;
    }

    public static string ToMediaType(StorageMediaCategory category) =>
        category switch
        {
            StorageMediaCategory.Image => MediaTypes.Image,
            StorageMediaCategory.Video => MediaTypes.Video,
            StorageMediaCategory.Document => MediaTypes.Document,
            StorageMediaCategory.Audio => MediaTypes.Audio,
            StorageMediaCategory.Other => MediaTypes.Other,
            _ => throw MediaErrors.InvalidMedia("The media category is not supported.")
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
