// File: backend/Services/Media/MediaService.Application/Services/Actors/IActorValidationService.cs
// Mục đích: Định nghĩa port kiểm tra actor có tồn tại và được phép thực hiện thao tác với media hay không.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.Services.Actors;

public interface IActorValidationService
{
    Task<ActorReference> ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken);
}
