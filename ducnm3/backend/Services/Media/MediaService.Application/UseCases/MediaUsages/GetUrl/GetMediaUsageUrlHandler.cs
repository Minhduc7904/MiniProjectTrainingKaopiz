// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrl/GetMediaUsageUrlHandler.cs
// Mục đích: Điều phối use case GetMediaUsageUrlHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Repositories;
using MediaService.Application.Services.Urls;
using MediaService.Application.UseCases.MediaUsages.GetUrls;
using MediaService.Domain.Constants;

using MediaService.Application.Common.Errors;

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
