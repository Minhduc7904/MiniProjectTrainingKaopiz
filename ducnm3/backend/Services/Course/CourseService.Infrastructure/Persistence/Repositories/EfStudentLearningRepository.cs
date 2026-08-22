using CourseService.Application.UseCases.Learning;
using CourseService.Infrastructure.Persistence.Scaffolded;
using CourseService.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfStudentLearningRepository(CourseDbContext db) : IStudentLearningRepository
{
    public async Task<StudentEnrollmentsResult> GetEnrollmentsAsync(Guid studentId, GetStudentEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        var source = db.Enrollments.AsNoTracking().Where(item => item.StudentId == studentId);
        var totalItems = await source.LongCountAsync(cancellationToken);
        var rows = await source
            .OrderByDescending(item => item.EnrolledAt)
            .ThenByDescending(item => item.Id)
            .Skip(checked((query.Page - 1) * query.PageSize))
            .Take(query.PageSize)
            .Select(item => new EnrollmentRow(item.Id, item.CourseId, item.Course.Name, item.Course.Status, item.Course.CreatedAt, item.EnrolledAt))
            .ToListAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : checked((int)Math.Ceiling(totalItems / (double)query.PageSize));
        return new StudentEnrollmentsResult(rows.Select(row => new StudentEnrollmentListItem(
            row.EnrollmentId, row.CourseId, row.CourseName, row.CourseStatus, ToUtc(row.CourseCreatedAt), ToUtc(row.EnrolledAt))).ToArray(), totalItems, totalPages);
    }

    public Task<bool> IsEnrolledAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) =>
        db.Enrollments.AsNoTracking().AnyAsync(item => item.CourseId == courseId && item.StudentId == studentId, cancellationToken);

    public async Task<StudentCourseCatalogResult> GetCatalogAsync(Guid studentId, GetStudentCourseCatalogQuery query, CancellationToken cancellationToken)
    {
        var source = db.Courses.AsNoTracking()
            .Where(course => course.Status == CourseStatuses.Published)
            .Where(course => !db.Enrollments.Any(enrollment => enrollment.CourseId == course.Id && enrollment.StudentId == studentId));
        var totalItems = await source.LongCountAsync(cancellationToken);
        var rows = await source
            .OrderByDescending(course => course.CreatedAt)
            .ThenByDescending(course => course.Id)
            .Skip(checked((query.Page - 1) * query.PageSize))
            .Take(query.PageSize)
            .Select(course => new CourseCatalogRow(course.Id, course.Name, course.Status, course.CreatedAt))
            .ToListAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : checked((int)Math.Ceiling(totalItems / (double)query.PageSize));
        return new StudentCourseCatalogResult(rows.Select(row => new StudentCourseCatalogItem(
            row.Id, row.Name, row.Status, ToUtc(row.CreatedAt))).ToArray(), totalItems, totalPages);
    }

    public async Task<StudentCourseDetailResult?> GetDetailAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken)
    {
        var course = await db.Courses.AsNoTracking()
            .Where(item => item.Id == courseId)
            .Select(item => new CourseRow(item.Id, item.Name, item.DescriptionMarkdown, item.Status, item.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);
        if (course is null) return null;
        var lessonRows = await (
            from lesson in db.Lessons.AsNoTracking()
            join progress in db.LessonProgresses.AsNoTracking().Where(item => item.StudentId == studentId)
                on lesson.Id equals progress.LessonId into progressGroup
            from progress in progressGroup.DefaultIfEmpty()
            where lesson.CourseId == courseId
            orderby lesson.DisplayOrder
            select new StudentLessonPreview(
                lesson.Id,
                lesson.Title,
                lesson.DisplayOrder,
                progress == null ? 0 : progress.ProgressPercent,
                progress == null ? null : progress.CompletedAt))
            .ToListAsync(cancellationToken);
        var lessons = lessonRows.Select(item => item.CompletedAtUtc is null
            ? item
            : item with { CompletedAtUtc = ToUtc(item.CompletedAtUtc.Value) }).ToArray();
        return new StudentCourseDetailResult(course.Id, course.Name, course.DescriptionMarkdown, course.Status, ToUtc(course.CreatedAt), lessons);
    }

    public async Task<StudentCourseProgressResult?> GetProgressAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken)
    {
        if (!await db.Courses.AsNoTracking().AnyAsync(item => item.Id == courseId, cancellationToken)) return null;
        var lessons = await db.Lessons.AsNoTracking()
            .Where(item => item.CourseId == courseId)
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new StudentLessonPreview(item.Id, item.Title, item.DisplayOrder, 0, null))
            .ToListAsync(cancellationToken);
        var lessonIds = lessons.Select(item => item.Id).ToArray();
        var completedLessonIds = lessonIds.Length == 0
            ? []
            : await db.LessonProgresses.AsNoTracking()
                .Where(item => lessonIds.Contains(item.LessonId) && item.StudentId == studentId && item.ProgressPercent >= 100 && item.CompletedAt != null)
                .Select(item => item.LessonId)
                .ToListAsync(cancellationToken);
        var completed = completedLessonIds.ToHashSet();
        var completedLessons = completed.Count;
        var totalLessons = lessons.Count;
        var progress = totalLessons == 0 ? 0 : Math.Round(completedLessons * 100m / totalLessons, 2, MidpointRounding.AwayFromZero);
        return new StudentCourseProgressResult(courseId, totalLessons, completedLessons, progress, lessons.FirstOrDefault(item => !completed.Contains(item.Id)));
    }

    private static DateTime ToUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private sealed record EnrollmentRow(Guid EnrollmentId, Guid CourseId, string CourseName, string CourseStatus, DateTime CourseCreatedAt, DateTime EnrolledAt);
    private sealed record CourseRow(Guid Id, string Name, string? DescriptionMarkdown, string Status, DateTime CreatedAt);
    private sealed record CourseCatalogRow(Guid Id, string Name, string Status, DateTime CreatedAt);
}
