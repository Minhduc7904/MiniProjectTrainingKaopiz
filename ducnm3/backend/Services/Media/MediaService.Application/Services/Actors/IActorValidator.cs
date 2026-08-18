// File: backend/Services/Media/MediaService.Application/Services/Actors/IActorValidator.cs
// Mục đích: Kiểm tra tính hợp lệ của dữ liệu đầu vào cho IActorValidator.

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
