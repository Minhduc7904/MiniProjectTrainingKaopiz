// File: backend/Services/Media/MediaService.UnitTests/TestDoubles/StubMediaRepository.cs
// Mục đích: Cung cấp test double StubMediaRepository để unit test cô lập use case khỏi dependency bên ngoài.

using MediaService.Application.Repositories;
using MediaService.Domain.Constants;
using MediaService.Domain.Entities;

namespace MediaService.UnitTests.TestDoubles;

public sealed class StubMediaRepository(List<string>? sharedEvents = null)
    : IMediaRepository, IMediaUsageRepository
{
    public List<string> Events { get; } = sharedEvents ?? [];

    public PendingMediaRecord? Pending { get; private set; }

    public MediaRecord? ExistingMedia { get; set; }

    public CreateMediaUsageRecord? CreatedUsage { get; private set; }

    public MediaUsageUrlRecord? ExistingUsageUrl { get; set; }

    public IReadOnlyList<MediaUsageUrlRecord> UsageUrls { get; set; } = [];

    public MediaUsageOwnerQuery? LastUsageUrlQuery { get; private set; }

    public IReadOnlyList<CreateMediaUsageRecord> EnsuredUsages { get; private set; } = [];

    public IReadOnlyList<CourseContentMediaUsageRemoval> RemovedCourseContentUsages { get; private set; } = [];

    public Task<IReadOnlyList<MediaLibraryRecord>> ListByActorAsync(
        Domain.ValueObjects.ActorReference actor,
        string? mediaType,
        (DateTime CreatedAtUtc, Guid Id)? cursor,
        int take,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<MediaLibraryRecord>>([]);

    public Task EnsureMediaUsagesAsync(
        IReadOnlyList<CreateMediaUsageRecord> usages,
        CancellationToken cancellationToken)
    {
        EnsuredUsages = usages;
        return Task.CompletedTask;
    }

    public Task EnsureCourseLessonMediaAsync(
        IReadOnlyList<CreateMediaUsageRecord> usages,
        CancellationToken cancellationToken) => EnsureMediaUsagesAsync(usages, cancellationToken);

    public Task RemoveCourseContentMediaAsync(
        IReadOnlyList<CourseContentMediaUsageRemoval> removals,
        CancellationToken cancellationToken)
    {
        RemovedCourseContentUsages = removals;
        return Task.CompletedTask;
    }

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
            null,
            IsDraft: media.IsDraft,
            DraftedAtUtc: media.DraftedAtUtc);
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
            IsDraft = true,
            DraftedAtUtc = completedAtUtc,
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

    public Task<MediaUsageUrlRecord?> GetActiveUsageUrlByIdAsync(
        Guid usageId,
        CancellationToken cancellationToken) =>
        Task.FromResult(
            ExistingUsageUrl?.Usage.Id == usageId
                ? ExistingUsageUrl
                : null);

    public Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(
        MediaUsageOwnerQuery query,
        CancellationToken cancellationToken)
    {
        LastUsageUrlQuery = query;
        return Task.FromResult(UsageUrls);
    }

    public Task<MediaUsage> ReplaceStudentAvatarAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
        => ReplaceUsageAsync(usage);

    public Task<MediaUsage> ReplaceMediaThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
        => ReplaceUsageAsync(usage);

    public Task<MediaUsage> ReplaceCourseThumbnailAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
        => ReplaceUsageAsync(usage);

    public Task<MediaUsage> AddCourseGalleryMediaAsync(
        CreateMediaUsageRecord usage,
        CancellationToken cancellationToken)
        => ReplaceUsageAsync(usage);

    private Task<MediaUsage> ReplaceUsageAsync(
        CreateMediaUsageRecord usage)
    {
        CreatedUsage = usage;
        return Task.FromResult(
            new MediaUsage(
                usage.Id,
                usage.MediaId,
                usage.OwnerService,
                usage.OwnerType,
                usage.OwnerId,
                usage.UsageType,
                usage.DisplayOrder,
                usage.CreatedBy,
                DateTime.UtcNow));
    }
}
