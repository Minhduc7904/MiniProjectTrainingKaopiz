// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrl/GetMediaUsageUrlHandler.cs
// Mục đích: Điều phối use case GetMediaUsageUrlHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Urls;
using MediaService.Application.UseCases.MediaUsages.GetUrls;
using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.MediaUsages.GetUrl;

public sealed class GetMediaUsageUrlHandler(
    IMediaUsageRepository mediaUsageRepository,
    IMediaUrlProvider mediaUrlProvider)
{
    public async Task<MediaUsageUrlResult> HandleAsync(
        GetMediaUsageUrlQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.UsageId == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia("usageId must be a valid UUID.");
        }

        var record = await mediaUsageRepository.GetActiveUsageUrlByIdAsync(
            query.UsageId,
            cancellationToken) ?? throw MediaErrors.MediaUsageNotFound();
        ValidateImage(record.Media);
        var url = await mediaUrlProvider.GenerateAsync(
            record.Media,
            cancellationToken);
        var thumbnailUrl = record.ThumbnailMedia is null
            ? null
            : await mediaUrlProvider.GenerateAsync(record.ThumbnailMedia, cancellationToken);
        return new MediaUsageUrlResult(
            record.Usage.Id,
            record.Media.Id,
            record.Usage.OwnerId,
            url.Value,
            thumbnailUrl?.Value,
            url.ExpiresAtUtc,
            record.Usage.DisplayOrder,
            record.Media.MediaType,
            record.Media.ContentType,
            record.Media.OriginalFileName);
    }

    internal static void ValidateImage(MediaRecord media)
    {
        if (media.Status != MediaObjectStatuses.Ready ||
            media.DeletedAtUtc is not null ||
            !string.Equals(
                media.MediaType,
                MediaTypes.Image,
                StringComparison.Ordinal))
        {
            throw MediaErrors.MediaNotReady();
        }
    }
}
