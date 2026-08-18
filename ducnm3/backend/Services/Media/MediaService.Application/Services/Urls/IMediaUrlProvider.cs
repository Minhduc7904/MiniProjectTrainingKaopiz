// File: backend/Services/Media/MediaService.Application/Services/Urls/IMediaUrlProvider.cs
// Mục đích: Định nghĩa port tạo URL truy cập nội dung media mà Application dùng nhưng không phụ thuộc HTTP endpoint cụ thể.

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
