// File: backend/Services/Media/MediaService.Application/Services/Actors/IActorValidationService.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.Services.Actors;

public interface IActorValidationService
{
    Task<ActorReference> ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken);
}
