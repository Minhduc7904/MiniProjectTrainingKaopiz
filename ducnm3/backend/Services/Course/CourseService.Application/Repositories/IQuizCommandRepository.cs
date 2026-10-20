// File: backend/Services/Course/CourseService.Application/Repositories/IQuizCommandRepository.cs
// Mục đích: Port soạn Quiz (admin): tạo, đọc, sửa, xóa cây Quiz → Question → Option.

namespace CourseService.Application.Repositories;

public interface IQuizCommandRepository
{
    Task<bool> LessonExistsAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken);

    Task<IReadOnlyList<QuizSummaryRecord>> ListAsync(Guid lessonId, CancellationToken cancellationToken);

    Task<QuizRecord?> GetAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken);

    Task<QuizRecord> CreateAsync(Guid lessonId, QuizDraft draft, CancellationToken cancellationToken);

    /// <summary>
    /// Cập nhật Quiz. Khi <see cref="QuizDraft.Questions"/> khác null thì thay toàn bộ cây câu hỏi;
    /// ném <c>QUIZ_HAS_ATTEMPTS</c> nếu Quiz đã có bài làm.
    /// </summary>
    Task<QuizRecord?> UpdateAsync(Guid lessonId, Guid quizId, QuizDraft draft, CancellationToken cancellationToken);

    Task<bool> HasAttemptsAsync(Guid quizId, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetQuizIdsByLessonAsync(Guid lessonId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetQuizIdsByCourseAsync(Guid courseId, CancellationToken cancellationToken);
}
