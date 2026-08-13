using BuildingBlocks.Contracts.Students;

namespace MediaService.Application.Abstractions.Clients;

public interface IStudentLookup
{
    Task<StudentQueryResponse?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken);
}
