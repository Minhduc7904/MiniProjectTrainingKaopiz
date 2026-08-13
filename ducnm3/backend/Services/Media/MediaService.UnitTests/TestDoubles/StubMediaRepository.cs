using MediaService.Application.Abstractions.Persistence;
using MediaService.Domain.Media;

namespace MediaService.UnitTests.TestDoubles;

public sealed class StubMediaRepository(List<string>? sharedEvents = null)
    : IMediaRepository
{
    public List<string> Events { get; } = sharedEvents ?? [];

    public PendingMediaRecord? Pending { get; private set; }

    public MediaRecord? ExistingMedia { get; set; }

    public CreateMediaUsageRecord? CreatedUsage { get; private set; }

    public Task AddPendingAsync(
        PendingMediaRecord media,
        CancellationToken cancellationToken)
    {
        Events.Add("pending");
        Pending = media;
        ExistingMedia = new MediaRecord(
            media.Id,
            media.Location,
            media.MediaType,
            media.ContentType,
            media.OriginalFileName,
            media.SizeBytes,
            MediaObjectStatuses.Pending,
            DateTime.UtcNow,
            null);
        return Task.CompletedTask;
    }

    public Task MarkReadyAsync(
        Guid mediaId,
        string checksumSha256,
        DateTime completedAtUtc,
        CancellationToken cancellationToken)
    {
        Events.Add("ready");
        ExistingMedia = ExistingMedia! with
        {
            Status = MediaObjectStatuses.Ready,
        };
        return Task.CompletedTask;
    }

    public Task MarkFailedAsync(
        Guid mediaId,
        string failureReason,
        CancellationToken cancellationToken)
    {
        Events.Add("failed");
        ExistingMedia = ExistingMedia is null
            ? null
            : ExistingMedia with { Status = MediaObjectStatuses.Failed };
        return Task.CompletedTask;
    }

    public Task<MediaRecord?> GetByIdAsync(
        Guid mediaId,
        CancellationToken cancellationToken) =>
        Task.FromResult(
            ExistingMedia?.Id == mediaId
                ? ExistingMedia
                : null);

    public Task<MediaUsageRecord> ReplaceStudentAvatarAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
        => ReplaceUsageAsync(usage);

    public Task<MediaUsageRecord> ReplaceMediaThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
        => ReplaceUsageAsync(usage);

    private Task<MediaUsageRecord> ReplaceUsageAsync(
        CreateMediaUsageRecord usage)
    {
        CreatedUsage = usage;
        return Task.FromResult(
            new MediaUsageRecord(
                usage.Id,
                usage.MediaId,
                usage.OwnerService,
                usage.OwnerType,
                usage.OwnerId,
                usage.UsageType,
                usage.DisplayOrder,
                DateTime.UtcNow));
    }
}
