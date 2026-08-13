using System.Text.RegularExpressions;
using MediaService.Application.Storage;

namespace MediaService.Infrastructure.Storage.Minio;

public static partial class MinioStorageRequestValidator
{
    public static ValidatedStorageUpload ValidateUpload(StorageUploadRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.Content.CanRead)
        {
            throw new StorageValidationException("The upload stream must be readable.");
        }

        if (request.Size <= 0)
        {
            throw new StorageValidationException("The upload size must be greater than zero.");
        }

        if (request.Content.CanSeek && request.Content.Length - request.Content.Position != request.Size)
        {
            throw new StorageValidationException("The declared upload size does not match the remaining stream length.");
        }

        ValidateLocation(request.Location, []);
        var contentType = NormalizeContentType(request.ContentType);
        return new ValidatedStorageUpload(contentType);
    }

    public static void ValidateLocation(
        StorageObjectLocation location,
        IReadOnlyCollection<string> allowedBuckets)
    {
        ArgumentNullException.ThrowIfNull(location);

        if (allowedBuckets.Count > 0 &&
            !allowedBuckets.Contains(location.Bucket, StringComparer.Ordinal))
        {
            throw new StorageValidationException("The storage bucket is not configured for Media Service.");
        }

        if (string.IsNullOrWhiteSpace(location.ObjectKey) ||
            location.ObjectKey.StartsWith('/') ||
            location.ObjectKey.Contains("..", StringComparison.Ordinal) ||
            location.ObjectKey.Any(char.IsControl))
        {
            throw new StorageValidationException("The storage object key is invalid.");
        }
    }

    private static string NormalizeContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new StorageValidationException("Content type is required.");
        }

        var normalized = contentType.Split(';', 2)[0].Trim().ToLowerInvariant();
        if (!ContentTypePattern().IsMatch(normalized))
        {
            throw new StorageValidationException("Content type is invalid.");
        }

        return normalized;
    }

    public static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new StorageValidationException("File extension is required.");
        }

        var normalized = extension.Trim().TrimStart('.').ToLowerInvariant();
        if (!ExtensionPattern().IsMatch(normalized))
        {
            throw new StorageValidationException(
                "File extension must contain 1-16 lowercase letters or numbers.");
        }

        return normalized;
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9!#$&^_.+-]*/[a-z0-9][a-z0-9!#$&^_.+-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ContentTypePattern();

    [GeneratedRegex("^[a-z0-9]{1,16}$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtensionPattern();
}

public sealed record ValidatedStorageUpload(string ContentType);
