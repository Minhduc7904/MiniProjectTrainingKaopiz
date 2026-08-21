using CourseService.Application.UseCases.Learning;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfLearningCommandRepository(CourseDbContext db) : ILearningCommandRepository
{
    public Task<CourseLearningCourse?> GetCourseAsync(Guid courseId, CancellationToken cancellationToken) =>
        db.Courses.AsNoTracking().Where(item => item.Id == courseId)
            .Select(item => new CourseLearningCourse(item.Id, item.Status)).SingleOrDefaultAsync(cancellationToken);

    public Task<bool> LessonBelongsToCourseAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken) =>
        db.Lessons.AsNoTracking().AnyAsync(item => item.Id == lessonId && item.CourseId == courseId, cancellationToken);

    public Task<EnrollmentResult?> GetEnrollmentAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) =>
        db.Enrollments.AsNoTracking().Where(item => item.CourseId == courseId && item.StudentId == studentId)
            .Select(item => new EnrollmentResult(item.Id, item.CourseId, item.StudentId, item.EnrolledAt)).SingleOrDefaultAsync(cancellationToken);

    public async Task<EnrollmentResult> CreateEnrollmentAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken)
    {
        var enrollment = new Enrollment { Id = Guid.NewGuid(), CourseId = courseId, StudentId = studentId, EnrolledAt = DateTime.UtcNow };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync(cancellationToken);
        return new EnrollmentResult(enrollment.Id, enrollment.CourseId, enrollment.StudentId, enrollment.EnrolledAt);
    }

    public async Task<LessonProgressResult> CompleteLessonAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken)
    {
        var progress = await db.LessonProgresses.SingleOrDefaultAsync(item => item.LessonId == lessonId && item.StudentId == studentId, cancellationToken);
        if (progress is null)
        {
            var now = DateTime.UtcNow;
            progress = new LessonProgress { Id = Guid.NewGuid(), LessonId = lessonId, StudentId = studentId, ProgressPercent = 100, CompletedAt = now, UpdatedAt = now };
            db.LessonProgresses.Add(progress);
        }
        else if (progress.ProgressPercent != 100 || progress.CompletedAt is null)
        {
            progress.ProgressPercent = 100;
            progress.CompletedAt ??= DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(cancellationToken);
        return new LessonProgressResult(progress.Id, progress.LessonId, progress.StudentId, progress.ProgressPercent, progress.CompletedAt ?? DateTime.UtcNow, progress.UpdatedAt);
    }
}
