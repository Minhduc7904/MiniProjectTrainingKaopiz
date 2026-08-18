// File: backend/Services/Media/MediaService.Application/UseCases/MediaDerivations/GenerateThumbnail/MediaThumbnailPolicy.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.MediaDerivations.GenerateThumbnail;

public static class MediaThumbnailPolicy
{
    public static bool RequiresThumbnail(string mediaType, string contentType) =>
        string.Equals(mediaType, MediaTypes.Image, StringComparison.Ordinal) ||
        string.Equals(mediaType, MediaTypes.Video, StringComparison.Ordinal) ||
        (string.Equals(mediaType, MediaTypes.Document, StringComparison.Ordinal) &&
         string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase));
}
