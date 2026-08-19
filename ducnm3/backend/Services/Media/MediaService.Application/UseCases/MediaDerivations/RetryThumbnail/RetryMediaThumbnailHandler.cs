// File: backend/Services/Media/MediaService.Application/UseCases/MediaDerivations/RetryThumbnail/RetryMediaThumbnailHandler.cs
// Mục đích: Điều phối use case RetryMediaThumbnailHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Application.UseCases.MediaDerivations.GetThumbnailStatus;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.MediaDerivations.RetryThumbnail;

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
