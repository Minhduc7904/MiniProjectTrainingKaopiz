using MediaService.Application.Abstractions.Persistence;

namespace MediaService.Application.Abstractions.Urls;

public interface IMediaUrlProvider
{
    Task<MediaUrl> GenerateAsync(
        MediaRecord media,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MediaUrl>> GenerateManyAsync(
        IReadOnlyList<MediaRecord> media,
        CancellationToken cancellationToken);
}
