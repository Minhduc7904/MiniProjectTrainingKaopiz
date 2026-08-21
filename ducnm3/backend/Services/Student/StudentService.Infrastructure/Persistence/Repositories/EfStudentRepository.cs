using Microsoft.EntityFrameworkCore;
using StudentService.Application.Repositories;
using StudentService.Application.UseCases.Students.GetList;
using StudentService.Infrastructure.Persistence.Mappers;
using StudentService.Infrastructure.Persistence.Scaffolded;
using StudentDomain = StudentService.Domain.Entities.Student;

namespace StudentService.Infrastructure.Persistence.Repositories;

public sealed class EfStudentRepository(StudentDbContext dbContext)
    : IStudentRepository, IStudentListRepository
{
    public async Task<StudentDomain?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == studentId, cancellationToken);
        return student is null ? null : StudentPersistenceMapper.ToDomain(student);
    }

    public async Task<StudentListPage> GetListAsync(GetStudentsQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.Students.AsNoTracking();
        if (query.Status is not null)
        {
            source = source.Where(student => student.Status == query.Status);
        }

        var totalItems = await source.LongCountAsync(cancellationToken);
        var rows = await ApplyStableOrder(source, query)
            .Skip(checked((query.Page - 1) * query.PageSize))
            .Take(query.PageSize)
            .Select(student => new StudentListRecord(
                student.Id,
                student.Email,
                student.DisplayName,
                student.Status,
                student.CreatedAt))
            .ToListAsync(cancellationToken);
        var items = rows
            .Select(student => student with { CreatedAtUtc = DateTime.SpecifyKind(student.CreatedAtUtc, DateTimeKind.Utc) })
            .ToArray();
        var totalPages = totalItems == 0 ? 0 : checked((int)Math.Ceiling(totalItems / (double)query.PageSize));
        return new StudentListPage(items, totalItems, totalPages);
    }

    public async Task<StudentDomain?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
        return student is null ? null : StudentPersistenceMapper.ToDomain(student);
    }

    public async Task<StudentDomain> CreateAsync(string normalizedEmail, string displayName, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            DisplayName = displayName,
            Status = "ACTIVE",
            CreatedAt = now,
            UpdatedAt = now,
        };
        dbContext.Students.Add(student);
        await dbContext.SaveChangesAsync(cancellationToken);
        return StudentPersistenceMapper.ToDomain(student);
    }

    private static IOrderedQueryable<Student> ApplyStableOrder(IQueryable<Student> source, GetStudentsQuery query) =>
        (query.SortBy, query.Descending) switch
        {
            (StudentSortField.CreatedAt, true) => source.OrderByDescending(student => student.CreatedAt).ThenByDescending(student => student.Id),
            (StudentSortField.CreatedAt, false) => source.OrderBy(student => student.CreatedAt).ThenBy(student => student.Id),
            (StudentSortField.DisplayName, true) => source.OrderByDescending(student => student.DisplayName).ThenByDescending(student => student.Id),
            (StudentSortField.DisplayName, false) => source.OrderBy(student => student.DisplayName).ThenBy(student => student.Id),
            (StudentSortField.Email, true) => source.OrderByDescending(student => student.Email).ThenByDescending(student => student.Id),
            _ => source.OrderBy(student => student.Email).ThenBy(student => student.Id),
        };
}
