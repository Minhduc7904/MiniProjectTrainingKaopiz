namespace MediaService.Application.Abstractions.Persistence;

using MediaService.Contracts.Messaging;

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

    Task<MediaUsageUrlRecord?> GetActiveUsageUrlByIdAsync(
        Guid usageId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(
        MediaUsageOwnerQuery query,
        CancellationToken cancellationToken);

    Task<MediaUsageRecord> ReplaceStudentAvatarAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken);

    Task<MediaUsageRecord> ReplaceMediaThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken);

    Task EnsureNotificationBodyUsagesAsync(
        Guid notificationId,
        Guid createdBy,
        IReadOnlyList<NotificationMediaUsageReferenceV1> references,
        CancellationToken cancellationToken);
}
