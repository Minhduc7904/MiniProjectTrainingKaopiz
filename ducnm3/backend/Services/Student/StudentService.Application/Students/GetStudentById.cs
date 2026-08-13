using BuildingBlocks.Contracts.Api;

namespace StudentService.Application.Students;

public sealed record StudentDetails(
    Guid Id,
    string Email,
    string DisplayName,
    string Status);

public interface IStudentRepository
{
    Task<StudentDetails?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken);
}

public sealed class GetStudentByIdHandler(IStudentRepository studentRepository)
{
    public async Task<StudentDetails> HandleAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (studentId == Guid.Empty)
        {
            throw new StudentApplicationException(
                "VALIDATION_ERROR",
                "studentId must be a valid UUID.",
                400);
        }

        return await studentRepository.GetByIdAsync(studentId, cancellationToken) ??
            throw new StudentApplicationException(
                "STUDENT_NOT_FOUND",
                "Student was not found.",
                404);
    }
}

public sealed class StudentApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode)
    : ApiException(errorCode, safeMessage, statusCode);
