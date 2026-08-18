// File: backend/Services/Media/MediaService.UnitTests/TestDoubles/StubActorValidationService.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Services.Actors;
using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.UnitTests.TestDoubles;

public sealed class StubActorValidationService : IActorValidationService
{
    public Task<ActorReference> ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken) =>
        Task.FromResult(actor.Normalize());
}
