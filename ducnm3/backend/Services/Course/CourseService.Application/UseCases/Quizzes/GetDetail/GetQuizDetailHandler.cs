// File: backend/Services/Course/CourseService.Application/UseCases/Quizzes/GetDetail/GetQuizDetailHandler.cs
// Mục đích: Đọc Quiz đầy đủ (kèm đáp án đúng) và danh sách Quiz của Lesson cho admin.

using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Quizzes.GetDetail;

public sealed class GetQuizDetailHandler(IQuizCommandRepository repository)
{
    public async Task<QuizRecord> HandleAsync(Guid courseId, Guid lessonId, Guid quizId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty || quizId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        if (!await repository.LessonExistsAsync(courseId, lessonId, cancellationToken))
        {
            throw CourseErrors.LessonNotFound();
        }

        return await repository.GetAsync(lessonId, quizId, cancellationToken) ?? throw CourseErrors.QuizNotFound();
    }
}
