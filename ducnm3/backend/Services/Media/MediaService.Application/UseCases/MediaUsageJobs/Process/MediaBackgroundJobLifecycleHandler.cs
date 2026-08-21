// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/Process/MediaBackgroundJobLifecycleHandler.cs
// Mục đích: Điều phối trạng thái processing, completed và failed cho job Markdown sync và usage delete.

using MediaService.Application.Repositories;
using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.MediaUsageJobs.Process;

public sealed class MediaBackgroundJobLifecycleHandler(
    IMediaBackgroundJobLifecycleRepository repository)
{
    public Task StartMarkdownSyncAsync(
        Guid jobId,
        string subjectType,
        Guid subjectId,
        uint expectedItemCount,
        CancellationToken cancellationToken) =>
        repository.StartAsync(
            jobId,
            MediaBackgroundJobTypes.MarkdownUsageSync,
            subjectType,
            subjectId,
            null,
            expectedItemCount,
            "{\"version\":1}",
            cancellationToken);

    public Task StartUsageDeleteAsync(
        Guid jobId,
        uint expectedItemCount,
        CancellationToken cancellationToken) =>
        repository.StartAsync(
            jobId,
            MediaBackgroundJobTypes.MediaUsageDelete,
            "MEDIA_USAGE_BATCH",
            jobId,
            jobId,
            expectedItemCount,
            "{\"version\":1}",
            cancellationToken);

    public Task CompleteAsync(Guid jobId, uint processedItemCount, CancellationToken cancellationToken) =>
        repository.CompleteAsync(jobId, processedItemCount, cancellationToken);

    public Task FailAsync(Guid jobId, uint failedItemCount, string safeError, CancellationToken cancellationToken) =>
        repository.FailAsync(jobId, failedItemCount, safeError, cancellationToken);
}
