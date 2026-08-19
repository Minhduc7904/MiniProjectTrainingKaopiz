// File: backend/Services/Media/MediaService.Application/Services/Actors/StudentActorValidator.cs
// Mục đích: Kiểm tra tính hợp lệ của dữ liệu đầu vào cho StudentActorValidator.

using MediaService.Application.Common.Errors;
using MediaService.Application.Services.Students;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.Services.Actors;

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
