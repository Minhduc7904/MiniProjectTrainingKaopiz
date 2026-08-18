// File: backend/Services/Media/MediaService.Application/Services/Actors/IActorValidator.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.Services.Actors;

public interface IActorValidator
{
    string ActorType { get; }

    Task ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken);
}
