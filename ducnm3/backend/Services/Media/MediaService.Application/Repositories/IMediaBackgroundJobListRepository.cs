// File: backend/Services/Media/MediaService.Application/Repositories/IMediaBackgroundJobListRepository.cs
// Mục đích: Định nghĩa port đọc danh sách Media background job đã lọc và phân trang.

using MediaService.Application.UseCases.MediaUsageJobs.GetList;

namespace MediaService.Application.Repositories;

public interface IMediaBackgroundJobListRepository
{
    Task<MediaBackgroundJobListPage> ListAsync(
        MediaBackgroundJobListRequest request,
        CancellationToken cancellationToken);
}
