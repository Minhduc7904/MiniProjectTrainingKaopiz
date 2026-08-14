using BuildingBlocks.Contracts.Api;

namespace MediaService.Application;

public static class MediaErrors
{
    public static MediaApplicationException InvalidMedia(string message) =>
        new(MediaErrorCodes.InvalidMedia, message, 400);

    public static MediaApplicationException UnsupportedMediaType() =>
        new(
            MediaErrorCodes.UnsupportedMediaType,
            "The content type does not match mediaType.",
            415);

    public static MediaApplicationException PayloadTooLarge() =>
        new(
            ApiErrorCodes.PayloadTooLarge,
            ApiErrorMessages.PayloadTooLarge,
            413);

    public static MediaApplicationException UploadFailed() =>
        new(
            MediaErrorCodes.MediaUploadFailed,
            "Media upload failed.",
            503);

    public static MediaApplicationException MediaNotFound() =>
        new(MediaErrorCodes.MediaNotFound, "Media was not found.", 404);

    public static MediaApplicationException MediaNotReady() =>
        new(
            MediaErrorCodes.MediaNotReady,
            "Media is not ready for use.",
            409);

    public static MediaApplicationException MediaUsageConflict() =>
        new(
            MediaErrorCodes.MediaUsageConflict,
            "The media usage conflicts with an active usage.",
            409);

    public static MediaApplicationException MediaUsageNotFound() =>
        new(
            MediaErrorCodes.MediaUsageNotFound,
            "The media usage was not found.",
            404);

    public static MediaApplicationException ThumbnailNotFound() =>
        new(
            MediaErrorCodes.ThumbnailNotFound,
            "A thumbnail operation was not found for this media.",
            404);

    public static MediaApplicationException ThumbnailRetryConflict() =>
        new(
            MediaErrorCodes.ThumbnailRetryConflict,
            "Only a failed thumbnail operation can be retried.",
            409);

    public static MediaApplicationException InvalidActorType(string message) =>
        new(MediaErrorCodes.InvalidActorType, message, 400);

    public static MediaApplicationException ActorNotFound() =>
        new(MediaErrorCodes.ActorNotFound, "The actor does not exist.", 404);

    public static MediaApplicationException OwnerNotFound() =>
        new(
            MediaErrorCodes.OwnerNotFound,
            "The media usage owner does not exist.",
            404);

    public static MediaApplicationException StudentServiceUnavailable() =>
        new(
            MediaErrorCodes.StudentServiceUnavailable,
            "Student Service is unavailable.",
            503);

    public static MediaApplicationException StorageUnavailable() =>
        new(
            ApiErrorCodes.StorageUnavailable,
            ApiErrorMessages.StorageUnavailable,
            503);
}
