// File: backend/Services/Course/CourseService.Application/Repositories/IStudentQuizRepository.cs
// Mục đích: Port học viên làm Quiz: xem đề đã publish, bắt đầu/tiếp tục, lưu nháp, nộp và xem lại bài làm.

namespace CourseService.Application.Repositories;

public interface IStudentQuizRepository
{
    Task<IReadOnlyList<StudentQuizSummaryRecord>> ListPublishedAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>Quiz PUBLISHED thuộc Lesson, kèm đáp án đúng (chỉ dùng nội bộ để chấm, không trả cho học viên).</summary>
    Task<QuizRecord?> GetPublishedAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken);

    /// <summary>Trả bài đang làm dở của học viên nếu có, ngược lại tạo bài làm mới.</summary>
    Task<QuizAttemptStartResult> StartAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken);

    Task<QuizAttemptRecord?> GetAttemptAsync(Guid quizId, Guid attemptId, Guid studentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<QuizAttemptSummaryRecord>> ListAttemptsAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>Thay toàn bộ đáp án nháp; trả null nếu bài làm không còn ở trạng thái IN_PROGRESS.</summary>
    Task<QuizAttemptRecord?> SaveAnswersAsync(Guid attemptId, Guid studentId, IReadOnlyList<QuizAnswerSelection> answers, CancellationToken cancellationToken);

    /// <summary>Ghi đáp án cuối (nếu có), chuyển SUBMITTED và lưu điểm; trả null nếu bài làm không còn IN_PROGRESS.</summary>
    Task<QuizAttemptRecord?> SubmitAsync(
        Guid attemptId,
        Guid studentId,
        IReadOnlyList<QuizAnswerSelection>? finalAnswers,
        decimal score,
        decimal maxScore,
        CancellationToken cancellationToken);
}
