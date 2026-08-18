// File: backend/Services/Media/MediaService.Application/UseCases/Media/Upload/MediaUploadOptions.cs
// Mục đích: Định nghĩa options cấu hình được bind từ application configuration cho MediaUploadOptions.

using MediaService.Application.Services.Storage;

namespace MediaService.Application.UseCases.Media.Upload;

public sealed class MediaUploadOptions
{
    public const string SectionName = "MediaUpload";

    public long ImageMaxBytes { get; init; } = 10 * 1024 * 1024;

    public long VideoMaxBytes { get; init; } = 500 * 1024 * 1024;

    public long DocumentMaxBytes { get; init; } = 50 * 1024 * 1024;

    public long AudioMaxBytes { get; init; } = 100 * 1024 * 1024;

    public long OtherMaxBytes { get; init; } = 25 * 1024 * 1024;

    public long RequestMaxBytes { get; init; } = 525 * 1024 * 1024;

    public long GetMaxBytes(StorageMediaCategory category) =>
        category switch
        {
            StorageMediaCategory.Image => ImageMaxBytes,
            StorageMediaCategory.Video => VideoMaxBytes,
            StorageMediaCategory.Document => DocumentMaxBytes,
            StorageMediaCategory.Audio => AudioMaxBytes,
            StorageMediaCategory.Other => OtherMaxBytes,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

    public void Validate()
    {
        if (ImageMaxBytes <= 0 ||
            VideoMaxBytes <= 0 ||
            DocumentMaxBytes <= 0 ||
            AudioMaxBytes <= 0 ||
            OtherMaxBytes <= 0 ||
            RequestMaxBytes < VideoMaxBytes)
        {
            throw new InvalidOperationException(
                "MediaUpload limits must be positive and request max must cover video max.");
        }
    }
}
