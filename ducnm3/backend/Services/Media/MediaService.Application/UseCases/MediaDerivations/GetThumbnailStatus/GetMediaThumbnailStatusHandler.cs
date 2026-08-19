// File: backend/Services/Media/MediaService.Application/UseCases/MediaDerivations/GetThumbnailStatus/GetMediaThumbnailStatusHandler.cs
// Mục đích: Điều phối use case GetMediaThumbnailStatusHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;

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
