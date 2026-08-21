using StudentService.Domain.Entities;

namespace StudentService.Application.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken);

    Task<Student?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<Student> CreateAsync(string normalizedEmail, string displayName, CancellationToken cancellationToken);
}
