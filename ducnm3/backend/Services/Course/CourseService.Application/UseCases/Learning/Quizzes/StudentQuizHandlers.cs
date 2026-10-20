// File: backend/Services/Course/CourseService.Application/UseCases/Learning/Quizzes/StudentQuizHandlers.cs
// Mục đích: Use case học viên làm Quiz: xem đề, bắt đầu/tiếp tục, lưu nháp, nộp bài và xem lại các lần làm.

using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Quizzes;

namespace CourseService.Application.UseCases.Learning.Quizzes;

/// <summary>Kiểm tra ID hợp lệ, học viên đã ghi danh và Lesson thuộc Course trước mọi thao tác Quiz của học viên.</summary>
public sealed class StudentQuizAccess(IStudentLearningRepository learningRepository, ILessonCommandRepository lessonRepository)
{
    public async Task EnsureAsync(Guid courseId, Guid lessonId, Guid studentId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty || studentId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        if (!await learningRepository.IsEnrolledAsync(courseId, studentId, cancellationToken))
        {
            throw CourseErrors.StudentNotEnrolled();
        }

        _ = await lessonRepository.GetAsync(courseId, lessonId, cancellationToken) ?? throw CourseErrors.LessonNotFound();
    }
}

public sealed class GetStudentQuizzesHandler(StudentQuizAccess access, IStudentQuizRepository repository)
{
    public async Task<IReadOnlyList<StudentQuizSummaryRecord>> HandleAsync(
        Guid courseId, Guid lessonId, Guid studentId, CancellationToken cancellationToken)
    {
        await access.EnsureAsync(courseId, lessonId, studentId, cancellationToken);
        return await repository.ListPublishedAsync(lessonId, studentId, cancellationToken);
    }
}

public sealed class GetStudentQuizDetailHandler(StudentQuizAccess access, IStudentQuizRepository repository)
{
    public async Task<QuizRecord> HandleAsync(
        Guid courseId, Guid lessonId, Guid quizId, Guid studentId, CancellationToken cancellationToken)
    {
        await access.EnsureAsync(courseId, lessonId, studentId, cancellationToken);
        if (quizId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        return await repository.GetPublishedAsync(lessonId, quizId, cancellationToken) ?? throw CourseErrors.QuizNotFound();
    }
}

public sealed class StartQuizAttemptHandler(StudentQuizAccess access, IStudentQuizRepository repository)
{
    public async Task<QuizAttemptStartResult> HandleAsync(
        Guid courseId, Guid lessonId, Guid quizId, Guid studentId, CancellationToken cancellationToken)
    {
        await access.EnsureAsync(courseId, lessonId, studentId, cancellationToken);
        if (quizId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        var quiz = await repository.GetPublishedAsync(lessonId, quizId, cancellationToken) ?? throw CourseErrors.QuizNotFound();
        return await repository.StartAsync(quiz.Id, studentId, cancellationToken);
    }
}

public sealed class SaveQuizAnswersHandler(StudentQuizAccess access, IStudentQuizRepository repository)
{
    public async Task<QuizAttemptRecord> HandleAsync(
        Guid courseId,
        Guid lessonId,
        Guid quizId,
        Guid attemptId,
        Guid studentId,
        IReadOnlyList<QuizAnswerInput>? answers,
        CancellationToken cancellationToken)
    {
        await access.EnsureAsync(courseId, lessonId, studentId, cancellationToken);
        if (quizId == Guid.Empty || attemptId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        var quiz = await repository.GetPublishedAsync(lessonId, quizId, cancellationToken) ?? throw CourseErrors.QuizNotFound();
        var attempt = await repository.GetAttemptAsync(quiz.Id, attemptId, studentId, cancellationToken)
            ?? throw CourseErrors.QuizAttemptNotFound();
        if (attempt.Status != Domain.Constants.QuizAttemptStatuses.InProgress) throw CourseErrors.QuizAttemptAlreadySubmitted();

        var selections = QuizAnswerValidation.Normalize(quiz, answers);
        return await repository.SaveAnswersAsync(attempt.Id, studentId, selections, cancellationToken)
            ?? throw CourseErrors.QuizAttemptAlreadySubmitted();
    }
}

public sealed record SubmitQuizAttemptResult(QuizAttemptRecord Attempt, QuizGrade Grade);

public sealed class SubmitQuizAttemptHandler(StudentQuizAccess access, IStudentQuizRepository repository)
{
    /// <param name="answers">Đáp án cuối; null nghĩa là dùng đáp án nháp đã lưu.</param>
    public async Task<SubmitQuizAttemptResult> HandleAsync(
        Guid courseId,
        Guid lessonId,
        Guid quizId,
        Guid attemptId,
        Guid studentId,
        IReadOnlyList<QuizAnswerInput>? answers,
        CancellationToken cancellationToken)
    {
        await access.EnsureAsync(courseId, lessonId, studentId, cancellationToken);
        if (quizId == Guid.Empty || attemptId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        var quiz = await repository.GetPublishedAsync(lessonId, quizId, cancellationToken) ?? throw CourseErrors.QuizNotFound();
        var attempt = await repository.GetAttemptAsync(quiz.Id, attemptId, studentId, cancellationToken)
            ?? throw CourseErrors.QuizAttemptNotFound();
        if (attempt.Status != Domain.Constants.QuizAttemptStatuses.InProgress) throw CourseErrors.QuizAttemptAlreadySubmitted();

        IReadOnlyList<QuizAnswerSelection>? finalAnswers = answers is null ? null : QuizAnswerValidation.Normalize(quiz, answers);
        var grade = QuizGrader.Grade(quiz, finalAnswers ?? attempt.Answers);
        var submitted = await repository.SubmitAsync(attempt.Id, studentId, finalAnswers, grade.Score, grade.MaxScore, cancellationToken)
            ?? throw CourseErrors.QuizAttemptAlreadySubmitted();
        return new SubmitQuizAttemptResult(submitted, grade);
    }
}

public sealed class GetQuizAttemptsHandler(StudentQuizAccess access, IStudentQuizRepository repository)
{
    public async Task<IReadOnlyList<QuizAttemptSummaryRecord>> HandleAsync(
        Guid courseId, Guid lessonId, Guid quizId, Guid studentId, CancellationToken cancellationToken)
    {
        await access.EnsureAsync(courseId, lessonId, studentId, cancellationToken);
        if (quizId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        var quiz = await repository.GetPublishedAsync(lessonId, quizId, cancellationToken) ?? throw CourseErrors.QuizNotFound();
        return await repository.ListAttemptsAsync(quiz.Id, studentId, cancellationToken);
    }
}

public sealed record QuizAttemptDetailResult(QuizAttemptRecord Attempt, QuizGrade? Grade);

public sealed class GetQuizAttemptDetailHandler(StudentQuizAccess access, IStudentQuizRepository repository)
{
    public async Task<QuizAttemptDetailResult> HandleAsync(
        Guid courseId, Guid lessonId, Guid quizId, Guid attemptId, Guid studentId, CancellationToken cancellationToken)
    {
        await access.EnsureAsync(courseId, lessonId, studentId, cancellationToken);
        if (quizId == Guid.Empty || attemptId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        var quiz = await repository.GetPublishedAsync(lessonId, quizId, cancellationToken) ?? throw CourseErrors.QuizNotFound();
        var attempt = await repository.GetAttemptAsync(quiz.Id, attemptId, studentId, cancellationToken)
            ?? throw CourseErrors.QuizAttemptNotFound();
        var grade = attempt.Status == Domain.Constants.QuizAttemptStatuses.Submitted ? QuizGrader.Grade(quiz, attempt.Answers) : null;
        return new QuizAttemptDetailResult(attempt, grade);
    }
}
