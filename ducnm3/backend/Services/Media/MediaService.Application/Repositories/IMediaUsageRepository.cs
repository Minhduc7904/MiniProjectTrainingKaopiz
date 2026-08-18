// File: backend/Services/Media/MediaService.Application/Repositories/IMediaUsageRepository.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.Entities;

namespace MediaService.Application.Repositories;

public interface IMediaUsageRepository
{
    Task<MediaUsageUrlRecord?> GetActiveUsageUrlByIdAsync(
        Guid usageId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(
        MediaUsageOwnerQuery query,
        CancellationToken cancellationToken);

    Task<MediaUsage> ReplaceStudentAvatarAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken);

    Task<MediaUsage> ReplaceMediaThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken);

    Task EnsureMediaUsagesAsync(
        IReadOnlyList<CreateMediaUsageRecord> usages,
        CancellationToken cancellationToken);
}
