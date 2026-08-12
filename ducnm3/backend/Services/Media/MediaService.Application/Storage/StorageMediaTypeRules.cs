namespace MediaService.Application.Storage;

public static class StorageMediaTypeRules
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

    public static bool Matches(StorageMediaCategory category, string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        var normalizedContentType = contentType.Trim().ToLowerInvariant();

        return category switch
        {
            StorageMediaCategory.Image => normalizedContentType.StartsWith("image/", StringComparison.Ordinal),
            StorageMediaCategory.Video => normalizedContentType.StartsWith("video/", StringComparison.Ordinal),
            StorageMediaCategory.Document => DocumentContentTypes.Contains(normalizedContentType),
            StorageMediaCategory.Audio => normalizedContentType.StartsWith("audio/", StringComparison.Ordinal),
            StorageMediaCategory.Other => !IsKnownCategory(normalizedContentType),
            _ => false
        };
    }

    private static bool IsKnownCategory(string contentType) =>
        contentType.StartsWith("image/", StringComparison.Ordinal) ||
        contentType.StartsWith("video/", StringComparison.Ordinal) ||
        contentType.StartsWith("audio/", StringComparison.Ordinal) ||
        DocumentContentTypes.Contains(contentType);
}
