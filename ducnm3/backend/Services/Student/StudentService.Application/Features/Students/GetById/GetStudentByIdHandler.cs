using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Students;

namespace StudentService.Application.Features.Students.GetById;

public sealed class GetStudentByIdHandler(IStudentRepository studentRepository)
{
    public async Task<StudentQueryResponse> HandleAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (studentId == Guid.Empty)
        {
            throw new StudentApplicationException(
                ApiErrorCodes.ValidationFailed,
                ApiErrorMessages.ValidationFailed,
                400);
        }

        return await studentRepository.GetByIdAsync(studentId, cancellationToken) ??
            throw new StudentApplicationException(
                StudentErrorCodes.NotFound,
                StudentErrorMessages.NotFound,
                404);
    }
}
