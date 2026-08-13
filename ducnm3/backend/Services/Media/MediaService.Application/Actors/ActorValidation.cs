using MediaService.Domain;

namespace MediaService.Application.Actors;

public interface IActorValidator
{
    string ActorType { get; }

    Task ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken);
}

public interface IActorValidationService
{
    Task<ActorReference> ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken);
}

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
            throw new MediaApplicationException(
                MediaErrorCodes.InvalidActorType,
                "Actor type and actor ID are required.",
                400);
        }

        if (!validatorsByType.TryGetValue(normalizedActor.Type, out var validator))
        {
            throw new MediaApplicationException(
                MediaErrorCodes.InvalidActorType,
                $"Actor type '{normalizedActor.Type}' is not supported.",
                400);
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

public sealed record StudentLookupResult(
    Guid Id,
    string Email,
    string DisplayName,
    string Status);

public interface IStudentLookup
{
    Task<StudentLookupResult?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken);
}

public sealed class StudentActorValidator(IStudentLookup studentLookup)
    : IActorValidator
{
    public string ActorType => ActorTypes.Student;

    public async Task ValidateAsync(
        ActorReference actor,
        CancellationToken cancellationToken)
    {
        var student = await studentLookup.GetByIdAsync(actor.Id, cancellationToken);
        if (student is null)
        {
            throw new MediaApplicationException(
                MediaErrorCodes.ActorNotFound,
                "The actor does not exist.",
                404);
        }
    }
}
