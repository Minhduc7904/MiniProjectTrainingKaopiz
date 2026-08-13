using MediaService.Application.Abstractions.Clients;
using MediaService.Domain.Actors;

namespace MediaService.Application.Actors;

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
            throw MediaErrors.ActorNotFound();
        }
    }
}
