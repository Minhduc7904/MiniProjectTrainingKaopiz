// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrls/GetMediaUsageUrlsHandler.cs
// Mục đích: Điều phối use case GetMediaUsageUrlsHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Urls;
using MediaService.Application.UseCases.MediaUsages.GetUrl;

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
        if (query.OwnerId == Guid.Empty ||
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
