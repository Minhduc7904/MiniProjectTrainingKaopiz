using StudentService.Application.Common.Errors;
using StudentService.Application.Repositories;

namespace StudentService.Application.UseCases.Students.GetById;

public sealed class GetStudentByIdHandler(IStudentRepository repository)
{
    public async Task<GetStudentByIdResult> HandleAsync(
        GetStudentByIdQuery query,
        CancellationToken cancellationToken)
    {
        if (query.StudentId == Guid.Empty)
        {
            throw StudentErrors.ValidationFailed();
        }

        var student = await repository.GetByIdAsync(query.StudentId, cancellationToken)
            ?? throw StudentErrors.NotFound();
        return new GetStudentByIdResult(student.Id, student.Email, student.DisplayName, student.Status);
    }
}
