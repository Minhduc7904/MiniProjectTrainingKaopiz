using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;

namespace MediaService.Application.UseCases.MediaUsages.GetIds;

public sealed class GetMediaUsageIdsByOwnersHandler(IMediaUsageRepository repository)
{
    public Task<IReadOnlyList<Guid>> HandleAsync(
        GetMediaUsageIdsByOwnersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Owners.Count == 0 || query.Owners.Any(owner =>
                owner.OwnerId == Guid.Empty ||
                string.IsNullOrWhiteSpace(owner.OwnerService) ||
                string.IsNullOrWhiteSpace(owner.OwnerType)))
        {
            throw MediaErrors.InvalidMedia("owners must contain ownerService, ownerType and ownerId.");
        }

        var owners = query.Owners
            .Select(owner => new MediaUsageOwnerScope(
                owner.OwnerService.Trim().ToUpperInvariant(),
                owner.OwnerType.Trim().ToUpperInvariant(),
                owner.OwnerId))
            .Distinct()
            .ToArray();
        return repository.GetActiveUsageIdsByOwnersAsync(owners, cancellationToken);
    }
}
