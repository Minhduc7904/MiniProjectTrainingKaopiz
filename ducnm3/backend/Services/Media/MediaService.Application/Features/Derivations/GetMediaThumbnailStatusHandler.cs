using MediaService.Application.Abstractions.Persistence;

namespace MediaService.Application.Features.Derivations;

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
