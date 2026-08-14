using BuildingBlocks.Contracts.Api;
using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Abstractions.Urls;

namespace MediaService.Application.Urls;

public sealed class ContentEndpointMediaUrlProvider : IMediaUrlProvider
{
    public Task<MediaUrl> GenerateAsync(
        MediaRecord media,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            new MediaUrl(ApiRoutes.Media.ContentPublicPath(media.Id), null));
    }

    public async Task<IReadOnlyList<MediaUrl>> GenerateManyAsync(
        IReadOnlyList<MediaRecord> media,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(media);
        var urls = new MediaUrl[media.Count];
        for (var index = 0; index < media.Count; index++)
        {
            urls[index] = await GenerateAsync(media[index], cancellationToken);
        }

        return urls;
    }
}
