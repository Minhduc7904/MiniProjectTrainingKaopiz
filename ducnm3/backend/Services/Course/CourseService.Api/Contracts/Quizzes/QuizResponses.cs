namespace CourseService.Api.Contracts.Quizzes;

/// <summary>Phía admin: có Markdown nguồn, HTML đã render và cờ đáp án đúng.</summary>
public sealed record QuizOptionResponse(Guid Id, string OptionMarkdown, string? OptionHtml, bool IsCorrect, uint DisplayOrder);

public sealed record QuizQuestionResponse(
    Guid Id,
    string QuestionMarkdown,
    string? QuestionHtml,
    string QuestionType,
    decimal Points,
    uint DisplayOrder,
    IReadOnlyList<QuizOptionResponse> Options);

public sealed record QuizResponse(
    Guid Id,
    Guid LessonId,
    string Title,
    string? DescriptionMarkdown,
    string? DescriptionHtml,
    uint DisplayOrder,
    string Status,
    int QuestionCount,
    decimal TotalPoints,
    bool HasAttempts,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<QuizQuestionResponse> Questions);

public sealed record QuizSummaryResponse(
    Guid Id,
    Guid LessonId,
    string Title,
    string? DescriptionMarkdown,
    string? DescriptionHtml,
    uint DisplayOrder,
    string Status,
    int QuestionCount,
    decimal TotalPoints,
    bool HasAttempts,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Phía học viên: chỉ có HTML đã render và không bao giờ có cờ đáp án đúng.</summary>
public sealed record StudentQuizOptionResponse(Guid Id, string? OptionHtml, uint DisplayOrder);

public sealed record StudentQuizQuestionResponse(
    Guid Id,
    string? QuestionHtml,
    string QuestionType,
    decimal Points,
    uint DisplayOrder,
    IReadOnlyList<StudentQuizOptionResponse> Options);

public sealed record StudentQuizResponse(
    Guid Id,
    string Title,
    string? DescriptionHtml,
    uint DisplayOrder,
    int QuestionCount,
    decimal TotalPoints,
    IReadOnlyList<StudentQuizQuestionResponse> Questions);

public sealed record StudentQuizSummaryResponse(
    Guid Id,
    string Title,
    string? DescriptionHtml,
    uint DisplayOrder,
    int QuestionCount,
    decimal TotalPoints,
    decimal? BestScore,
    decimal? BestMaxScore,
    int SubmittedAttempts,
    Guid? InProgressAttemptId);

public sealed record QuizAnswerResponse(Guid QuestionId, IReadOnlyList<Guid> OptionIds);

public sealed record QuizAttemptResponse(
    Guid Id,
    Guid QuizId,
    string Status,
    DateTime StartedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal? Score,
    decimal? MaxScore,
    int AnsweredQuestions,
    IReadOnlyList<QuizAnswerResponse> Answers);

public sealed record QuizAttemptSummaryResponse(
    Guid Id,
    string Status,
    DateTime StartedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal? Score,
    decimal? MaxScore,
    int AnsweredQuestions);

public sealed record QuizQuestionResultResponse(
    Guid QuestionId,
    bool Answered,
    bool Correct,
    decimal PointsEarned,
    decimal PointsPossible,
    IReadOnlyList<Guid> SelectedOptionIds,
    IReadOnlyList<Guid> CorrectOptionIds);

public sealed record QuizResultResponse(
    decimal Score,
    decimal MaxScore,
    int CorrectCount,
    int IncorrectCount,
    int UnansweredCount,
    IReadOnlyList<QuizQuestionResultResponse> Questions);

/// <summary><see cref="Result"/> chỉ có khi bài làm đã nộp.</summary>
public sealed record QuizAttemptDetailResponse(QuizAttemptResponse Attempt, QuizResultResponse? Result);
