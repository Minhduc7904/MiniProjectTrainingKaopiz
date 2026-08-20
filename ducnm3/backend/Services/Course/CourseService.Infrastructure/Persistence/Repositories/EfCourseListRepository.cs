// File: backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfCourseListRepository.cs
// Mục đích: Đọc Course qua EF Core theo đường toàn bộ hoặc offset pagination tách biệt.

using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetList;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfCourseListRepository(CourseDbContext dbContext) : ICourseListRepository
{
    public async Task<IReadOnlyList<CourseListItemRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await ApplyStableOrder(dbContext.Courses.AsNoTracking(), CourseSortField.CreatedAt, true)
            .Select(course => new CourseListRow(course.Id, course.Name, course.Status, course.CreatedAt))
            .ToListAsync(cancellationToken);
        return rows.Select(MapItem).ToArray();
    }

    public async Task<GetCoursesResult> GetPagedAsync(GetCoursesQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.Courses.AsNoTracking();
        if (query.Status is not null) source = source.Where(course => course.Status == query.Status);
        var totalItems = await source.LongCountAsync(cancellationToken);
        var rows = await ApplyStableOrder(source, query.SortBy, query.Descending)
            .Skip(checked((query.Page - 1) * query.PageSize))
            .Take(query.PageSize)
            .Select(course => new CourseListRow(course.Id, course.Name, course.Status, course.CreatedAt))
            .ToListAsync(cancellationToken);
        var items = rows.Select(MapItem).ToArray();
        var totalPages = totalItems == 0 ? 0 : checked((int)Math.Ceiling(totalItems / (double)query.PageSize));
        return new GetCoursesResult(items, totalItems, totalPages);
    }

    public async Task<CourseExportChunk> ReadExportChunkAsync(
        ExportCoursesQuery query,
        CourseExportPosition? position,
        CancellationToken cancellationToken)
    {
        var source = dbContext.Courses.AsNoTracking();
        if (query.Status is not null) source = source.Where(course => course.Status == query.Status);
        if (position is not null)
        {
            source = source.Where(course => course.CreatedAt < position.CreatedAtUtc ||
                (course.CreatedAt == position.CreatedAtUtc && course.Id.CompareTo(position.Id) < 0));
        }

        var remaining = query.Limit is int limit ? limit - (position?.ReadCount ?? 0) : ExportCoursesQuery.ChunkSize;
        if (remaining <= 0) return new CourseExportChunk([], position?.ReadCount ?? 0);

        var rows = await source
            .OrderByDescending(course => course.CreatedAt)
            .ThenByDescending(course => course.Id)
            .Take(Math.Min(ExportCoursesQuery.ChunkSize, remaining))
            .Select(course => new CourseExportRowProjection(
                course.Id, course.Name, course.Status, course.CreatedAt))
            .ToListAsync(cancellationToken);
        return new CourseExportChunk(
            rows.Select(row => new CourseExportRow(
                row.Id, row.Name, row.Status,
                DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc))).ToArray(),
            position?.ReadCount ?? 0);
    }

    public async Task<IReadOnlyList<CourseExportRow>> ReadAllExportRowsAsync(
        ExportCoursesQuery query,
        CancellationToken cancellationToken)
    {
        var source = dbContext.Courses.AsNoTracking();
        if (query.Status is not null) source = source.Where(course => course.Status == query.Status);

        var rows = await source
            .OrderByDescending(course => course.CreatedAt)
            .ThenByDescending(course => course.Id)
            .Take(query.Limit ?? int.MaxValue)
            .Select(course => new CourseExportRowProjection(
                course.Id, course.Name, course.Status, course.CreatedAt))
            .ToListAsync(cancellationToken);
        return rows.Select(row => new CourseExportRow(
            row.Id, row.Name, row.Status,
            DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc))).ToArray();
    }

    private static IOrderedQueryable<Course> ApplyStableOrder(IQueryable<Course> source, CourseSortField sortBy, bool descending) =>
        (sortBy, descending) switch
        {
            (CourseSortField.CreatedAt, true) => source.OrderByDescending(course => course.CreatedAt).ThenByDescending(course => course.Id),
            (CourseSortField.CreatedAt, false) => source.OrderBy(course => course.CreatedAt).ThenBy(course => course.Id),
            (CourseSortField.Name, true) => source.OrderByDescending(course => course.Name).ThenByDescending(course => course.Id),
            _ => source.OrderBy(course => course.Name).ThenBy(course => course.Id),
        };

    private static CourseListItemRecord MapItem(CourseListRow row) =>
        new(row.Id, row.Name, row.Status, DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc));

    private sealed record CourseListRow(Guid Id, string Name, string Status, DateTime CreatedAt);

    private sealed record CourseExportRowProjection(Guid Id, string Name, string Status, DateTime CreatedAt);
}
