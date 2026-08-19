// File: backend/Services/Media/MediaService.Infrastructure/Services/Urls/ContentEndpointMediaUrlProvider.cs
// Mục đích: Tạo URL nội bộ trỏ đến endpoint đọc content Media thay vì phát lộ trực tiếp key hay presigned storage URL.

using BuildingBlocks.Contracts.Api;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Urls;

namespace MediaService.Infrastructure.Services.Urls;

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
