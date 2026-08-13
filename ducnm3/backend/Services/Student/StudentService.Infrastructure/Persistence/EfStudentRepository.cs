using Microsoft.EntityFrameworkCore;
using StudentService.Application.Students;

namespace StudentService.Infrastructure.Persistence;

public sealed class EfStudentRepository(StudentDbContext dbContext)
    : IStudentRepository
{
    public Task<StudentDetails?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken) =>
        dbContext.Students
            .AsNoTracking()
            .Where(student => student.Id == studentId)
            .Select(student => new StudentDetails(
                student.Id,
                student.Email,
                student.DisplayName,
                student.Status))
            .SingleOrDefaultAsync(cancellationToken);
}
