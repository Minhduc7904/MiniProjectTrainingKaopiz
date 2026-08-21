using BuildingBlocks.Contracts.Api;
using StudentService.Application.Common.Errors;
using StudentService.Application.Repositories;
using StudentService.Domain.Constants;

namespace StudentService.Application.UseCases.Auth.Register;

public sealed class RegisterStudentHandler(IStudentRepository repository)
{
    public async Task<RegisterStudentResult> HandleAsync(RegisterStudentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var email = command.Email?.Trim().ToLowerInvariant();
        var displayName = command.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(email) || email.Length > 320 || !email.Contains('@', StringComparison.Ordinal) || string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200)
            throw StudentErrors.ValidationFailed();
        if (await repository.GetByNormalizedEmailAsync(email, cancellationToken) is not null)
            throw StudentErrors.EmailExists();
        var student = await repository.CreateAsync(email, displayName, cancellationToken);
        return new RegisterStudentResult(ActorHeaderTypes.Student, student.Id);
    }
}
