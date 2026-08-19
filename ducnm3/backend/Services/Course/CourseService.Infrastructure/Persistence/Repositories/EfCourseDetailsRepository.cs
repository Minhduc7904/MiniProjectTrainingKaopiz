using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.GetDetails;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfCourseDetailsRepository(CourseDbContext db) : ICourseDetailsRepository
{
    public async Task<CourseDetailsResult?> GetWithoutNPlusOneAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var course = await db.Courses
            .AsNoTracking()
            .Where(x => x.Id == courseId)
            .Select(x => new { x.Id, x.Name, x.Status, x.CreatedAt })
            .SingleOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return null;
        }

        var lessons = await db.Lessons
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new { x.Id, x.Title, x.DisplayOrder })
            .ToListAsync(cancellationToken);
        var lessonIds = lessons.Select(x => x.Id).ToArray();
        var progresses = lessonIds.Length == 0
            ? []
            : await db.LessonProgresses
                .AsNoTracking()
                .Where(x => lessonIds.Contains(x.LessonId))
                .Select(x => new
                {
                    x.LessonId,
                    x.StudentId,
                    x.ProgressPercent,
                    x.CompletedAt,
                    x.UpdatedAt
                })
                .ToListAsync(cancellationToken);

        var progressesByLesson = progresses.ToLookup(x => x.LessonId);
        return new CourseDetailsResult(
            course.Id,
            course.Name,
            course.Status,
            ToUtc(course.CreatedAt),
            lessons.Select(lesson => new LessonDetailsResult(
                lesson.Id,
                lesson.Title,
                lesson.DisplayOrder,
                progressesByLesson[lesson.Id].Select(progress => new LessonProgressDetailsResult(
                    progress.StudentId,
                    progress.ProgressPercent,
                    progress.CompletedAt is null ? null : ToUtc(progress.CompletedAt.Value),
                    ToUtc(progress.UpdatedAt))).ToArray())).ToArray());
    }

    // Benchmark-only reference implementation. Do not call from an HTTP endpoint.
    public async Task<CourseDetailsResult?> GetWithNPlusOneAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var course = await db.Courses
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == courseId, cancellationToken);
        if (course is null)
        {
            return null;
        }

        var lessons = await db.Lessons
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
        var output = new List<LessonDetailsResult>();
        foreach (var lesson in lessons)
        {
            var progresses = await db.LessonProgresses
                .AsNoTracking()
                .Where(x => x.LessonId == lesson.Id)
                .ToListAsync(cancellationToken);
            output.Add(new LessonDetailsResult(
                lesson.Id,
                lesson.Title,
                lesson.DisplayOrder,
                progresses.Select(progress => new LessonProgressDetailsResult(
                    progress.StudentId,
                    progress.ProgressPercent,
                    progress.CompletedAt is null ? null : ToUtc(progress.CompletedAt.Value),
                    ToUtc(progress.UpdatedAt))).ToArray()));
        }

        return new CourseDetailsResult(
            course.Id,
            course.Name,
            course.Status,
            ToUtc(course.CreatedAt),
            output);
    }

    private static DateTime ToUtc(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
