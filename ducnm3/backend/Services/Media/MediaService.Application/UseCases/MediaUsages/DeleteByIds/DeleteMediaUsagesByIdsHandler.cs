using MediaService.Application.Repositories;
using MediaService.Application.UseCases.MediaUsageJobs.Process;
using MediaService.Contracts.Messaging;

namespace MediaService.Application.UseCases.MediaUsages.DeleteByIds;

public sealed class DeleteMediaUsagesByIdsHandler(
    IMediaUsageRepository repository,
    MediaBackgroundJobLifecycleHandler jobLifecycleHandler)
{
    public async Task HandleAsync(DeleteMediaUsagesByIdsV1 command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var usageIds = command.UsageIds.Distinct().Where(id => id != Guid.Empty).ToArray();
        var jobId = command.JobId == Guid.Empty ? Guid.NewGuid() : command.JobId;
        await jobLifecycleHandler.StartUsageDeleteAsync(
            jobId, checked((uint)usageIds.Length), cancellationToken);
        await repository.RemoveByIdsAsync(usageIds, cancellationToken);
        await jobLifecycleHandler.CompleteAsync(jobId, checked((uint)usageIds.Length), cancellationToken);
    }
}
