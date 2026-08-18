// File: backend/Services/Media/MediaService.Application/UseCases/MediaDerivations/GetThumbnailStatus/GetMediaThumbnailStatusHandler.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Repositories;

using MediaService.Application.Common.Errors;

namespace MediaService.Application.UseCases.MediaDerivations.GetThumbnailStatus;

public sealed class GetMediaThumbnailStatusHandler(
    IMediaDerivationRepository repository)
{
    public async Task<MediaThumbnailStatusResult> HandleAsync(
        Guid sourceMediaId,
        CancellationToken cancellationToken)
    {
        if (sourceMediaId == Guid.Empty)
        {
            throw MediaErrors.ThumbnailNotFound();
        }

        var status = await repository.GetThumbnailStatusAsync(
            sourceMediaId,
            cancellationToken) ?? throw MediaErrors.ThumbnailNotFound();
        return MediaThumbnailStatusResult.FromRecord(status);
    }
}

public sealed record MediaThumbnailStatusResult(
    Guid SourceMediaId,
    Guid JobId,
    string Status,
    Guid ThumbnailMediaId,
    Guid? ActiveThumbnailMediaId,
    string? LastError,
    DateTime UpdatedAtUtc)
{
    public static MediaThumbnailStatusResult FromRecord(
        MediaThumbnailStatusRecord record) =>
        new(
            record.SourceMediaId,
            record.JobId,
            record.Status,
            record.ThumbnailMediaId,
            record.ActiveThumbnailMediaId,
            record.LastError,
            record.UpdatedAtUtc);
}
