using BuildingBlocks.Contracts.Students;
using Microsoft.EntityFrameworkCore;
using StudentService.Application.Features.Students.GetById;

namespace StudentService.Infrastructure.Persistence;

public sealed class EfStudentRepository(StudentDbContext dbContext)
    : IStudentRepository
{
    public Task<StudentQueryResponse?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken) =>
        dbContext.Students
            .AsNoTracking()
            .Where(student => student.Id == studentId)
            .Select(student => new StudentQueryResponse(
                student.Id,
                student.Email,
                student.DisplayName,
                student.Status))
            .SingleOrDefaultAsync(cancellationToken);
}
