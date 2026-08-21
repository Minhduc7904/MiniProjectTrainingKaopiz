using StudentService.Application.Repositories;
using StudentService.Application.UseCases.Auth.Login;

namespace StudentService.Application.UseCases.Auth.GetMe;

public sealed class GetCurrentStudentHandler(IStudentRepository repository)
{
    public async Task<GetCurrentStudentResult> HandleAsync(GetCurrentStudentQuery query, CancellationToken cancellationToken)
    {
        var student = await LoginStudentHandler.GetActiveStudentAsync(query.StudentId, repository, cancellationToken);
        return new GetCurrentStudentResult(student.Id, student.Email, student.DisplayName, student.Status);
    }
}
