// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrls/GetMediaUsageUrlsHandler.cs
// Mục đích: Điều phối use case GetMediaUsageUrlsHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Urls;
using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.MediaUsages.GetUrls;

public sealed class GetMediaUsageUrlsHandler(
    IMediaUsageRepository mediaUsageRepository,
    IMediaUrlProvider mediaUrlProvider)
{
    public async Task<IReadOnlyList<MediaUsageUrlResult>> HandleAsync(
        GetMediaUsageUrlsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.OwnerIds.Count == 0 || query.OwnerIds.Any(id => id == Guid.Empty) ||
            string.IsNullOrWhiteSpace(query.OwnerService) ||
            string.IsNullOrWhiteSpace(query.OwnerType) ||
            string.IsNullOrWhiteSpace(query.UsageType))
        {
            throw MediaErrors.InvalidMedia(
                "ownerService, ownerType, usageType and ownerId are required.");
        }

        var records = await mediaUsageRepository.GetActiveUsageUrlsAsync(
            new MediaUsageOwnerQuery(
                query.OwnerService.Trim().ToUpperInvariant(),
                query.OwnerType.Trim().ToUpperInvariant(),
                query.UsageType.Trim().ToUpperInvariant(),
                query.OwnerIds),
            cancellationToken);
        var readableRecords = records
            .Where(record => IsReadable(record.Media))
            .Select(record => IsReadableThumbnail(record.ThumbnailMedia)
                ? record
                : record with { ThumbnailMedia = null })
            .ToArray();

        var urls = await mediaUrlProvider.GenerateManyAsync(
            readableRecords.SelectMany(record => new[] { record.Media, record.ThumbnailMedia })
                .Where(media => media is not null)
                .Cast<MediaRecord>()
                .ToArray(),
            cancellationToken);
        var urlByMediaId = urls.Select((url, index) => new { url, mediaId = readableRecords.SelectMany(record => new[] { record.Media, record.ThumbnailMedia }).Where(media => media is not null).Cast<MediaRecord>().ElementAt(index).Id })
            .ToDictionary(item => item.mediaId, item => item.url);
        return readableRecords
            .Select(record => new MediaUsageUrlResult(
                    record.Usage.Id,
                    record.Media.Id,
                    record.Usage.OwnerId,
                    urlByMediaId[record.Media.Id].Value,
                    record.ThumbnailMedia is null ? null : urlByMediaId[record.ThumbnailMedia.Id].Value,
                    urlByMediaId[record.Media.Id].ExpiresAtUtc,
                    record.Usage.DisplayOrder,
                    record.Media.MediaType,
                    record.Media.ContentType,
                    record.Media.OriginalFileName))
            .ToArray();
    }

    private static bool IsReadable(MediaRecord? media) =>
        media is not null &&
        media.Status == MediaObjectStatuses.Ready &&
        media.DeletedAtUtc is null;

    private static bool IsReadableThumbnail(MediaRecord? media) =>
        IsReadable(media) && media!.MediaType == MediaTypes.Image;
}
