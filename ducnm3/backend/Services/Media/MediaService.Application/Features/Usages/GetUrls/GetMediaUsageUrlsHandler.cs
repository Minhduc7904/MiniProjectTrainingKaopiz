using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Abstractions.Urls;

namespace MediaService.Application.Features.Usages.GetUrls;

public sealed class GetMediaUsageUrlsHandler(
    IMediaRepository mediaRepository,
    IMediaUrlProvider mediaUrlProvider)
{
    public async Task<IReadOnlyList<MediaUsageUrlResult>> HandleAsync(
        GetMediaUsageUrlsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.OwnerId == Guid.Empty ||
            string.IsNullOrWhiteSpace(query.OwnerService) ||
            string.IsNullOrWhiteSpace(query.OwnerType) ||
            string.IsNullOrWhiteSpace(query.UsageType))
        {
            throw MediaErrors.InvalidMedia(
                "ownerService, ownerType, usageType and ownerId are required.");
        }

        var records = await mediaRepository.GetActiveUsageUrlsAsync(
            new MediaUsageOwnerQuery(
                query.OwnerService.Trim().ToUpperInvariant(),
                query.OwnerType.Trim().ToUpperInvariant(),
                query.UsageType.Trim().ToUpperInvariant(),
                query.OwnerId),
            cancellationToken);
        foreach (var record in records)
        {
            GetMediaUsageUrlHandler.ValidateImage(record.Media);
        }

        var urls = await mediaUrlProvider.GenerateManyAsync(
            records.Select(record => record.Media).ToArray(),
            cancellationToken);
        return records
            .Zip(
                urls,
                (record, url) => new MediaUsageUrlResult(
                    record.Usage.Id,
                    record.Media.Id,
                    url.Value,
                    url.ExpiresAtUtc,
                    record.Usage.DisplayOrder))
            .ToArray();
    }
}
