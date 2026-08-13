using BuildingBlocks.Contracts.Students;

namespace StudentService.Application.Features.Students.GetById;

public interface IStudentRepository
{
    Task<StudentQueryResponse?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken);
}
