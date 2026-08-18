// File: backend/Services/Media/MediaService.Application/Services/Actors/ActorValidationService.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

using MediaService.Application.Common.Errors;

namespace MediaService.Application.Services.Actors;

public sealed class ActorValidationService(IEnumerable<IActorValidator> validators)
    : IActorValidationService
{
    private readonly Dictionary<string, IActorValidator> validatorsByType =
        CreateValidatorMap(validators);

    public async Task<ActorReference> ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var normalizedActor = actor.Normalize();
        if (normalizedActor.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(normalizedActor.Type))
        {
            throw MediaErrors.InvalidActorType(
                "Actor type and actor ID are required.");
        }

        if (!validatorsByType.TryGetValue(normalizedActor.Type, out var validator))
        {
            throw MediaErrors.InvalidActorType(
                $"Actor type '{normalizedActor.Type}' is not supported.");
        }

        await validator.ValidateAsync(normalizedActor, cancellationToken);
        return normalizedActor;
    }

    private static Dictionary<string, IActorValidator> CreateValidatorMap(
        IEnumerable<IActorValidator> validators)
    {
        var result = new Dictionary<string, IActorValidator>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var validator in validators)
        {
            if (!result.TryAdd(validator.ActorType, validator))
            {
                throw new InvalidOperationException(
                    $"Multiple actor validators are registered for '{validator.ActorType}'.");
            }
        }

        return result;
    }
}
