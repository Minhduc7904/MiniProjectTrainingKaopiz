// File: backend/Services/Media/MediaService.Application/Repositories/IMediaRepository.cs
// Mục đích: Khai báo port repository IMediaRepository để use case truy cập dữ liệu mà không phụ thuộc EF Core.

namespace MediaService.Application.Repositories;

public interface IMediaRepository
{
    Task AddPendingAsync(
        PendingMediaRecord media,
        CancellationToken cancellationToken);

    Task MarkReadyAsync(
        Guid mediaId,
        string checksumSha256,
        DateTime completedAtUtc,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid mediaId,
        string failureReason,
        CancellationToken cancellationToken);

    Task<MediaRecord?> GetByIdAsync(
        Guid mediaId,
        CancellationToken cancellationToken);

}
