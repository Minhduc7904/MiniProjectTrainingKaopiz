// File: backend/Services/Media/MediaService.Application/Repositories/IMediaRepository.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

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
