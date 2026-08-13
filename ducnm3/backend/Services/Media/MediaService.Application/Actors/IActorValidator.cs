using MediaService.Domain.Actors;

namespace MediaService.Application.Actors;

public interface IActorValidator
{
    string ActorType { get; }

    Task ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken);
}
