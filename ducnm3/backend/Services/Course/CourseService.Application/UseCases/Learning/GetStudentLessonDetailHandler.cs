using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Learning;

public sealed class GetStudentLessonDetailHandler(
    IStudentLearningRepository learningRepository,
    ILessonCommandRepository lessonRepository)
{
    public async Task<LessonCreateRecord> HandleAsync(Guid courseId, Guid lessonId, Guid studentId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty || studentId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        if (!await learningRepository.IsEnrolledAsync(courseId, studentId, cancellationToken))
        {
            throw StudentNotEnrolled();
        }

        return await lessonRepository.GetAsync(courseId, lessonId, cancellationToken)
            ?? throw CourseErrors.LessonNotFound();
    }

    private static CourseApplicationException StudentNotEnrolled() =>
        new("STUDENT_NOT_ENROLLED", "Student is not enrolled in this course.", 403);
}
