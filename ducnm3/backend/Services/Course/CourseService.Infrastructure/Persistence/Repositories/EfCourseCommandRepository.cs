using CourseService.Application.Repositories;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfCourseCommandRepository(CourseDbContext db) : ICourseCommandRepository
{
    public async Task<CourseCommandRecord?> GetAsync(Guid courseId, CancellationToken cancellationToken) =>
        await db.Courses.AsNoTracking().Where(item => item.Id == courseId)
            .Select(item => new CourseCommandRecord(item.Id, item.Name, item.DescriptionMarkdown, item.Status, item.CreatedAt, item.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<CourseCommandRecord> CreateAsync(string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var course = new Course { Id = Guid.NewGuid(), Name = name, DescriptionMarkdown = descriptionMarkdown, Status = status, CreatedAt = now, UpdatedAt = now };
        db.Courses.Add(course);
        await db.SaveChangesAsync(cancellationToken);
        return new CourseCommandRecord(course.Id, course.Name, course.DescriptionMarkdown, course.Status, course.CreatedAt, course.UpdatedAt);
    }

    public async Task<CourseCommandRecord?> UpdateAsync(Guid courseId, string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken)
    {
        var course = await db.Courses.SingleOrDefaultAsync(item => item.Id == courseId, cancellationToken);
        if (course is null) return null;
        course.Name = name;
        course.DescriptionMarkdown = descriptionMarkdown;
        course.Status = status;
        course.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return new CourseCommandRecord(course.Id, course.Name, course.DescriptionMarkdown, course.Status, course.CreatedAt, course.UpdatedAt);
    }

    public async Task<IReadOnlyList<Guid>> GetLessonIdsAsync(Guid courseId, CancellationToken cancellationToken) =>
        await db.Lessons.AsNoTracking()
            .Where(item => item.CourseId == courseId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

    public async Task<bool> DeleteAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await db.Courses.SingleOrDefaultAsync(item => item.Id == courseId, cancellationToken);
        if (course is null)
        {
            return false;
        }

        db.Courses.Remove(course);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
