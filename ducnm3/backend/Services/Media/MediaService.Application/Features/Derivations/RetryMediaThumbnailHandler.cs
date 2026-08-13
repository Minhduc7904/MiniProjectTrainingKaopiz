using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Actors;
using MediaService.Domain.Actors;

namespace MediaService.Application.Features.Derivations;

public sealed class RetryMediaThumbnailHandler(
    IMediaDerivationRepository repository,
    IActorValidationService actorValidationService)
{
    public async Task<MediaThumbnailStatusResult> HandleAsync(
        Guid sourceMediaId,
        ActorReference actor,
        CancellationToken cancellationToken)
    {
        if (sourceMediaId == Guid.Empty)
        {
            throw MediaErrors.ThumbnailNotFound();
        }

        var validatedActor = await actorValidationService.ValidateAsync(
            actor,
            cancellationToken);
        var status = await repository.RetryAsync(
            sourceMediaId,
            validatedActor,
            cancellationToken);
        return MediaThumbnailStatusResult.FromRecord(status);
    }
}
