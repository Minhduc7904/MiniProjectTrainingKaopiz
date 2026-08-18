// File: backend/Services/Media/MediaService.Application/Services/Urls/IMediaUrlProvider.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Repositories;

namespace MediaService.Application.Services.Urls;

public interface IMediaUrlProvider
{
    Task<MediaUrl> GenerateAsync(
        MediaRecord media,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MediaUrl>> GenerateManyAsync(
        IReadOnlyList<MediaRecord> media,
        CancellationToken cancellationToken);
}
