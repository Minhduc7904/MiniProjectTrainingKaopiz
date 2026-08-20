using CourseService.Application.Repositories;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfLessonCommandRepository(CourseDbContext db) : ILessonCommandRepository
{
    public async Task<LessonCreateRecord?> CreateAsync(
        Guid courseId,
        string title,
        string? contentMarkdown,
        uint? displayOrder,
        CancellationToken cancellationToken)
    {
        var courseExists = await db.Courses.AnyAsync(item => item.Id == courseId, cancellationToken);
        if (!courseExists)
        {
            return null;
        }

        var nextOrder = displayOrder ?? ((await db.Lessons
            .Where(item => item.CourseId == courseId)
            .Select(item => (uint?)item.DisplayOrder)
            .MaxAsync(cancellationToken)) ?? 0) + (displayOrder is null ? 1u : 0u);
        var now = DateTime.UtcNow;
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            ContentMarkdown = contentMarkdown,
            DisplayOrder = nextOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(cancellationToken);
        return new LessonCreateRecord(
            lesson.Id,
            lesson.CourseId,
            lesson.Title,
            lesson.ContentMarkdown,
            lesson.DisplayOrder,
            lesson.CreatedAt,
            lesson.UpdatedAt);
    }
}
