using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Abstractions.Urls;
using MediaService.Domain.Media;

namespace MediaService.Application.Features.Usages.GetUrls;

public sealed class GetMediaUsageUrlHandler(
    IMediaRepository mediaRepository,
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

        var record = await mediaRepository.GetActiveUsageUrlByIdAsync(
            query.UsageId,
            cancellationToken) ?? throw MediaErrors.MediaUsageNotFound();
        ValidateImage(record.Media);
        var url = await mediaUrlProvider.GenerateAsync(
            record.Media,
            cancellationToken);
        return new MediaUsageUrlResult(
            record.Usage.Id,
            record.Media.Id,
            url.Value,
            url.ExpiresAtUtc,
            record.Usage.DisplayOrder);
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
