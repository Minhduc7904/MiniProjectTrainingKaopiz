using MediaService.Application.Repositories;
using MediaService.Contracts.Messaging;

namespace MediaService.Application.UseCases.MediaUsages.DeleteByIds;

public sealed class DeleteMediaUsagesByIdsHandler(IMediaUsageRepository repository)
{
    public Task HandleAsync(DeleteMediaUsagesByIdsV1 command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return repository.RemoveByIdsAsync(command.UsageIds, cancellationToken);
    }
}
