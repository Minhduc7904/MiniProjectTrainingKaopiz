using MediaService.Application.Common.Errors;
using MediaService.Application.Services.Admins;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.Services.Actors;

public sealed class AdminActorValidator(IAdminLookup adminLookup) : IActorValidator
{
    public string ActorType => ActorTypes.Admin;

    public async Task ValidateAsync(ActorReference actor, CancellationToken cancellationToken)
    {
        var admin = await adminLookup.GetByIdAsync(actor.Id, cancellationToken);
        if (admin is null) throw MediaErrors.ActorNotFound();
    }
}
