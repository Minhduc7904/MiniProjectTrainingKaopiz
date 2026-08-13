using MediaService.Domain.Media;

namespace MediaService.Application.Features.Derivations;

public static class MediaThumbnailPolicy
{
    public static bool RequiresThumbnail(string mediaType, string contentType) =>
        string.Equals(mediaType, MediaTypes.Image, StringComparison.Ordinal) ||
        string.Equals(mediaType, MediaTypes.Video, StringComparison.Ordinal) ||
        (string.Equals(mediaType, MediaTypes.Document, StringComparison.Ordinal) &&
         string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase));
}
