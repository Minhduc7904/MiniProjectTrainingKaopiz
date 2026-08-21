using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.Learning;

public interface ILearningCommandRepository
{
    Task<CourseLearningCourse?> GetCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task<bool> LessonBelongsToCourseAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken);
    Task<EnrollmentResult?> GetEnrollmentAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken);
    Task<EnrollmentResult> CreateEnrollmentAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken);
    Task<LessonProgressResult> CompleteLessonAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken);
}

public sealed record CourseLearningCourse(Guid Id, string Status);
public sealed record EnrollmentResult(Guid Id, Guid CourseId, Guid StudentId, DateTime EnrolledAtUtc);
public sealed record LessonProgressResult(Guid Id, Guid LessonId, Guid StudentId, decimal ProgressPercent, DateTime CompletedAtUtc, DateTime UpdatedAtUtc);

public sealed class EnrollCourseHandler(ILearningCommandRepository repository)
{
    public async Task<EnrollmentResult> HandleAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || studentId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        var course = await repository.GetCourseAsync(courseId, cancellationToken) ?? throw CourseErrors.CourseNotFound();
        if (!string.Equals(course.Status, CourseStatuses.Published, StringComparison.Ordinal))
            throw new CourseApplicationException("COURSE_NOT_ENROLLABLE", "Course is not available for enrollment.", 403);
        if (await repository.GetEnrollmentAsync(courseId, studentId, cancellationToken) is not null)
            throw new CourseApplicationException("ENROLLMENT_ALREADY_EXISTS", "Student is already enrolled in this course.", 409);
        return await repository.CreateEnrollmentAsync(courseId, studentId, cancellationToken);
    }
}

public sealed class CompleteLessonProgressHandler(ILearningCommandRepository repository)
{
    public async Task<LessonProgressResult> HandleAsync(Guid courseId, Guid lessonId, Guid studentId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty || studentId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        if (await repository.GetCourseAsync(courseId, cancellationToken) is null) throw CourseErrors.CourseNotFound();
        if (!await repository.LessonBelongsToCourseAsync(courseId, lessonId, cancellationToken)) throw CourseErrors.LessonNotFound();
        if (await repository.GetEnrollmentAsync(courseId, studentId, cancellationToken) is null)
            throw new CourseApplicationException("STUDENT_NOT_ENROLLED", "Student is not enrolled in this course.", 403);
        return await repository.CompleteLessonAsync(lessonId, studentId, cancellationToken);
    }
}
