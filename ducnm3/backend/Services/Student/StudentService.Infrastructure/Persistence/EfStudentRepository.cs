using BuildingBlocks.Contracts.Students;
using Microsoft.EntityFrameworkCore;
using StudentService.Application.Features.Students.GetById;
using StudentService.Application.Features.Students.GetList;
using StudentService.Infrastructure.Persistence.Scaffolded;

namespace StudentService.Infrastructure.Persistence;

public sealed class EfStudentRepository(StudentDbContext dbContext)
    : IStudentRepository, IStudentListRepository
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

    public async Task<GetStudentsResult> GetListAsync(
        GetStudentsQuery query,
        CancellationToken cancellationToken)
    {
        var source = dbContext.Students.AsNoTracking();
        if (query.Status is not null)
        {
            source = source.Where(student => student.Status == query.Status);
        }

        var totalItems = await source.LongCountAsync(cancellationToken);
        var ordered = ApplyStableOrder(source, query);
        var skip = checked((query.Page - 1) * query.PageSize);
        var rows = await ordered
            .Skip(skip)
            .Take(query.PageSize)
            .Select(student => new StudentListItem(
                student.Id,
                student.Email,
                student.DisplayName,
                student.Status,
                student.CreatedAt))
            .ToListAsync(cancellationToken);
        var items = rows
            .Select(student => student with
            {
                CreatedAtUtc = DateTime.SpecifyKind(
                    student.CreatedAtUtc,
                    DateTimeKind.Utc),
            })
            .ToArray();
        var totalPages = totalItems == 0
            ? 0
            : checked((int)Math.Ceiling(totalItems / (double)query.PageSize));

        return new GetStudentsResult(items, totalItems, totalPages);
    }

    private static IOrderedQueryable<Student> ApplyStableOrder(
        IQueryable<Student> source,
        GetStudentsQuery query) =>
        (query.SortBy, query.Descending) switch
        {
            (StudentSortField.CreatedAt, true) => source
                .OrderByDescending(student => student.CreatedAt)
                .ThenByDescending(student => student.Id),
            (StudentSortField.CreatedAt, false) => source
                .OrderBy(student => student.CreatedAt)
                .ThenBy(student => student.Id),
            (StudentSortField.DisplayName, true) => source
                .OrderByDescending(student => student.DisplayName)
                .ThenByDescending(student => student.Id),
            (StudentSortField.DisplayName, false) => source
                .OrderBy(student => student.DisplayName)
                .ThenBy(student => student.Id),
            (StudentSortField.Email, true) => source
                .OrderByDescending(student => student.Email)
                .ThenByDescending(student => student.Id),
            _ => source
                .OrderBy(student => student.Email)
                .ThenBy(student => student.Id),
        };
}
