// File: backend/Services/Media/MediaService.Application/UseCases/Media/GetContent/GetMediaContentHandler.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Domain.Constants;

using MediaService.Application.Common.Errors;

namespace MediaService.Application.UseCases.Media.GetContent;

public sealed class GetMediaContentHandler(
    IMediaRepository mediaRepository,
    IStorage storage)
{
    public async Task<GetMediaContentResult> HandleAsync(
        GetMediaContentQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.MediaId == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia("mediaId must be a valid UUID.");
        }

        var media = await mediaRepository.GetByIdAsync(
            query.MediaId,
            cancellationToken);
        if (media is null || media.DeletedAtUtc is not null)
        {
            throw MediaErrors.MediaNotFound();
        }

        if (media.Status != MediaObjectStatuses.Ready)
        {
            throw MediaErrors.MediaNotReady();
        }

        return new GetMediaContentResult(
            media.Id,
            media.MediaType,
            media.ContentType,
            media.OriginalFileName,
            media.SizeBytes,
            media.CreatedAtUtc,
            storage,
            media.Location);
    }
}
