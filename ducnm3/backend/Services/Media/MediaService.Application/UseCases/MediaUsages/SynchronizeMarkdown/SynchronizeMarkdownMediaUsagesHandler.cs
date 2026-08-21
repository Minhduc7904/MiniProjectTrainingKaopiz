// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/SynchronizeMarkdown/SynchronizeMarkdownMediaUsagesHandler.cs
// Mục đích: Đồng bộ add/remove media usage của Markdown theo owner được policy cho phép.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.MediaUsages;
using MediaService.Application.UseCases.MediaUsageJobs.Process;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.MediaUsages.SynchronizeMarkdown;

public sealed class SynchronizeMarkdownMediaUsagesHandler(
    IMediaUsageRepository repository,
    MediaBackgroundJobLifecycleHandler jobLifecycleHandler)
{
    public async Task HandleAsync(
        SynchronizeMarkdownMediaUsageV1 command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.OwnerId == Guid.Empty || command.CreatedBy == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia("The Markdown media usage command is invalid.");
        }

        MarkdownMediaUsageOwnerPolicy.Validate(command.OwnerService, command.OwnerType);
        foreach (var reference in command.Added.Concat(command.Removed))
        {
            MarkdownMediaUsageOwnerPolicy.ValidateReference(reference);
        }

        var jobId = command.JobId == Guid.Empty ? Guid.NewGuid() : command.JobId;
        var expectedItemCount = checked((uint)(command.Added.Count + command.Removed.Count));
        await jobLifecycleHandler.StartMarkdownSyncAsync(
            jobId, command.OwnerType, command.OwnerId, expectedItemCount, cancellationToken);

        var actor = new ActorReference(ActorTypes.Admin, command.CreatedBy);
        var additions = command.Added
            .GroupBy(reference => (reference.MediaId, reference.UsageType))
            .Select(group => group.First())
            .Select(reference => new CreateMediaUsageRecord(
                Guid.NewGuid(), reference.MediaId, command.OwnerService, command.OwnerType,
                command.OwnerId, reference.UsageType, reference.DisplayOrder, actor))
            .ToArray();
        if (additions.Length > 0)
        {
            await repository.EnsureCourseLessonMediaAsync(additions, cancellationToken);
        }

        var removals = command.Removed
            .GroupBy(reference => (reference.MediaId, reference.UsageType))
            .Select(group => group.First())
            .Select(reference => new CourseContentMediaUsageRemoval(
                command.OwnerId, command.OwnerType, reference.MediaId, reference.UsageType))
            .ToArray();
        if (removals.Length > 0)
        {
            await repository.RemoveCourseContentMediaAsync(removals, cancellationToken);
        }
        await jobLifecycleHandler.CompleteAsync(jobId, expectedItemCount, cancellationToken);
    }
}
