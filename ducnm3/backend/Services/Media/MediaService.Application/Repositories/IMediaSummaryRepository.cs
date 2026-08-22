// File: backend/Services/Media/MediaService.Application/Repositories/IMediaSummaryRepository.cs
// Mục đích: Khai báo port đọc tổng số media object cho dashboard quản trị.

namespace MediaService.Application.Repositories;

public interface IMediaSummaryRepository
{
    Task<long> CountAsync(CancellationToken cancellationToken);
}
