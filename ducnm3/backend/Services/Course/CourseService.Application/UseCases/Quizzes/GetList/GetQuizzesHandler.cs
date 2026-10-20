// File: backend/Services/Course/CourseService.Application/UseCases/Quizzes/GetList/GetQuizzesHandler.cs
// Mục đích: Liệt kê Quiz của một Lesson cho admin, kèm số câu và tổng điểm.

using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Quizzes.GetList;

public sealed class GetQuizzesHandler(IQuizCommandRepository repository)
{
    public async Task<IReadOnlyList<QuizSummaryRecord>> HandleAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        if (!await repository.LessonExistsAsync(courseId, lessonId, cancellationToken))
        {
            throw CourseErrors.LessonNotFound();
        }

        return await repository.ListAsync(lessonId, cancellationToken);
    }
}
