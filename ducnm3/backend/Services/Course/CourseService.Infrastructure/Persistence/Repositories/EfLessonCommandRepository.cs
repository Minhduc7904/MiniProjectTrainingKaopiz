using CourseService.Application.Repositories;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfLessonCommandRepository(CourseDbContext db) : ILessonCommandRepository
{
    public async Task<LessonCreateRecord?> GetAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken) =>
        await db.Lessons.AsNoTracking().Where(item => item.CourseId == courseId && item.Id == lessonId)
            .Select(item => new LessonCreateRecord(item.Id, item.CourseId, item.Title, item.ContentMarkdown, item.DisplayOrder, item.CreatedAt, item.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<LessonCreateRecord?> UpdateAsync(Guid courseId, Guid lessonId, string title, string? contentMarkdown, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.SingleOrDefaultAsync(item => item.CourseId == courseId && item.Id == lessonId, cancellationToken);
        if (lesson is null) return null;
        lesson.Title = title;
        lesson.ContentMarkdown = contentMarkdown;
        lesson.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return new LessonCreateRecord(lesson.Id, lesson.CourseId, lesson.Title, lesson.ContentMarkdown, lesson.DisplayOrder, lesson.CreatedAt, lesson.UpdatedAt);
    }

    public async Task<bool> ReorderAsync(Guid courseId, IReadOnlyList<Guid> lessonIds, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var lessons = await db.Lessons.Where(item => item.CourseId == courseId).ToListAsync(cancellationToken);
        if (lessons.Count != lessonIds.Count || lessonIds.Distinct().Count() != lessonIds.Count || lessons.Any(item => !lessonIds.Contains(item.Id))) return false;
        foreach (var lesson in lessons) lesson.DisplayOrder = checked(lesson.DisplayOrder + 1_000_000u);
        await db.SaveChangesAsync(cancellationToken);
        for (var index = 0; index < lessonIds.Count; index++) lessons.Single(item => item.Id == lessonIds[index]).DisplayOrder = checked((uint)index + 1);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

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
