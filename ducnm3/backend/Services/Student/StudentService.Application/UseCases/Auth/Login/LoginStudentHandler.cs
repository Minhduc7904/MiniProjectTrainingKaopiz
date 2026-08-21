using BuildingBlocks.Contracts.Api;
using StudentService.Application.Common.Errors;
using StudentService.Application.Repositories;
using StudentService.Domain.Constants;
using StudentService.Domain.Entities;

namespace StudentService.Application.UseCases.Auth.Login;

public sealed class LoginStudentHandler(IStudentRepository repository)
{
    public async Task<LoginStudentResult> HandleAsync(LoginStudentCommand command, CancellationToken cancellationToken)
    {
        var student = await GetActiveStudentAsync(command.StudentId, repository, cancellationToken);
        return new LoginStudentResult(ActorHeaderTypes.Student, student.Id);
    }

    internal static async Task<Student> GetActiveStudentAsync(Guid studentId, IStudentRepository repository, CancellationToken cancellationToken)
    {
        if (studentId == Guid.Empty) throw StudentErrors.ValidationFailed();
        var student = await repository.GetByIdAsync(studentId, cancellationToken) ?? throw StudentErrors.NotFound();
        if (!string.Equals(student.Status, StudentStatuses.Active, StringComparison.Ordinal)) throw StudentErrors.NotActive();
        return student;
    }
}
