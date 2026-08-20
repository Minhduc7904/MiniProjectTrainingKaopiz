// File: backend/Services/Media/MediaService.Application/Repositories/IMediaUsageRepository.cs
// Mục đích: Khai báo port repository IMediaUsageRepository để use case truy cập dữ liệu mà không phụ thuộc EF Core.

using MediaService.Domain.Entities;
using MediaService.Domain.ValueObjects;

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

    Task<MediaUsage> ReplaceCourseThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken);

    Task<MediaUsage> AddCourseGalleryMediaAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken);

    Task EnsureCourseLessonMediaAsync(
        IReadOnlyList<CreateMediaUsageRecord> usages,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task EnsureMediaUsagesAsync(
        IReadOnlyList<CreateMediaUsageRecord> usages,
        CancellationToken cancellationToken);

    Task RemoveAsync(Guid usageId, ActorReference actor, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task ReorderAsync(
        string ownerService,
        string ownerType,
        Guid ownerId,
        IReadOnlyList<Guid> usageIds,
        ActorReference actor,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}
