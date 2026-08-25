// File: backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfCourseListRepository.cs
// Mục đích: Đọc Course qua EF Core theo đường toàn bộ hoặc offset pagination tách biệt.

using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetList;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfCourseListRepository(CourseDbContext dbContext) : ICourseListRepository, ICourseSummaryRepository
{
    public async Task<(long TotalCourses, long TotalLessons)> CountAsync(CancellationToken cancellationToken)
    {
        var totalCourses = await dbContext.Courses.AsNoTracking().LongCountAsync(cancellationToken);
        var totalLessons = await dbContext.Lessons.AsNoTracking().LongCountAsync(cancellationToken);
        return (totalCourses, totalLessons);
    }

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
        if (query.Search is not null) source = source.Where(course => course.Name.Contains(query.Search));
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
        // AsNoTracking giảm memory/change tracking vì export chỉ đọc và không sửa Course.
        var source = dbContext.Courses.AsNoTracking();
        if (query.Status is not null) source = source.Where(course => course.Status == query.Status);
        if (position is not null)
        {
            // Keyset predicate cho thứ tự CreatedAt DESC, Id DESC: chỉ lấy các row "sau" row cuối của chunk trước.
            // Khác OFFSET, database không phải bỏ qua ngày càng nhiều row khi file export lớn.
            source = source.Where(course => course.CreatedAt < position.CreatedAtUtc ||
                (course.CreatedAt == position.CreatedAtUtc && course.Id.CompareTo(position.Id) < 0));
        }

        // remaining biến limit tổng của file thành giới hạn cho chunk hiện tại.
        var remaining = query.Limit is int limit ? limit - (position?.ReadCount ?? 0) : ExportCoursesQuery.ChunkSize;
        if (remaining <= 0) return new CourseExportChunk([], position?.ReadCount ?? 0);

        // EF Core dịch đoạn này thành một SELECT có WHERE/ORDER BY/LIMIT. Projection chỉ lấy cột cần cho CSV.
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
        // Đây là đường benchmark buffered, không dùng cho endpoint export streaming:
        // ToListAsync materialize toàn bộ kết quả (hoặc limit) trước khi caller ghi một byte response nào.
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
