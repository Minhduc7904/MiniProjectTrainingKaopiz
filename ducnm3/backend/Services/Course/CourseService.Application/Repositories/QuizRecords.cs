// File: backend/Services/Course/CourseService.Application/Repositories/QuizRecords.cs
// Mục đích: Record dữ liệu Quiz trao đổi giữa Application và Infrastructure (đọc, ghi, bài làm).

namespace CourseService.Application.Repositories;

public sealed record QuizOptionRecord(Guid Id, string OptionMarkdown, bool IsCorrect, uint DisplayOrder);

public sealed record QuizQuestionRecord(
    Guid Id,
    string QuestionMarkdown,
    string QuestionType,
    decimal Points,
    uint DisplayOrder,
    IReadOnlyList<QuizOptionRecord> Options);

/// <summary>Quiz kèm toàn bộ câu hỏi và lựa chọn (có cờ đáp án đúng).</summary>
public sealed record QuizRecord(
    Guid Id,
    Guid LessonId,
    string Title,
    string? DescriptionMarkdown,
    uint DisplayOrder,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    bool HasAttempts,
    IReadOnlyList<QuizQuestionRecord> Questions)
{
    public decimal TotalPoints => Questions.Sum(question => question.Points);
}

public sealed record QuizSummaryRecord(
    Guid Id,
    Guid LessonId,
    string Title,
    string? DescriptionMarkdown,
    uint DisplayOrder,
    string Status,
    int QuestionCount,
    decimal TotalPoints,
    bool HasAttempts,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record QuizOptionDraft(string OptionMarkdown, bool IsCorrect);

public sealed record QuizQuestionDraft(
    string QuestionMarkdown,
    string QuestionType,
    decimal Points,
    IReadOnlyList<QuizOptionDraft> Options);

/// <summary>Dữ liệu Quiz đã normalize và validate, sẵn sàng ghi xuống database.</summary>
public sealed record QuizDraft(
    string Title,
    string? DescriptionMarkdown,
    string Status,
    IReadOnlyList<QuizQuestionDraft>? Questions);

public sealed record StudentQuizSummaryRecord(
    Guid Id,
    string Title,
    string? DescriptionMarkdown,
    uint DisplayOrder,
    int QuestionCount,
    decimal TotalPoints,
    decimal? BestScore,
    decimal? BestMaxScore,
    int SubmittedAttempts,
    Guid? InProgressAttemptId);

public sealed record QuizAnswerSelection(Guid QuestionId, Guid OptionId);

public sealed record QuizAttemptRecord(
    Guid Id,
    Guid QuizId,
    string Status,
    DateTime StartedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal? Score,
    decimal? MaxScore,
    IReadOnlyList<QuizAnswerSelection> Answers);

public sealed record QuizAttemptSummaryRecord(
    Guid Id,
    string Status,
    DateTime StartedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal? Score,
    decimal? MaxScore,
    int AnsweredQuestions);

public sealed record QuizAttemptStartResult(QuizAttemptRecord Attempt, bool Created);
