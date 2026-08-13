using MediaService.Domain.Actors;

namespace MediaService.Application.Actors;

public interface IActorValidationService
{
    Task<ActorReference> ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken);
}
