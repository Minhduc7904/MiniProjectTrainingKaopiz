namespace CourseService.Api.Contracts.Quizzes;

public sealed record QuizOptionRequest(string? OptionMarkdown, bool? IsCorrect);

public sealed record QuizQuestionRequest(
    string? QuestionMarkdown,
    string? QuestionType,
    decimal? Points,
    IReadOnlyList<QuizOptionRequest>? Options);

public sealed record CreateQuizRequest(
    string? Title,
    string? DescriptionMarkdown,
    string? Status,
    IReadOnlyList<QuizQuestionRequest>? Questions);

/// <summary><see cref="Questions"/> null giữ nguyên cây câu hỏi; có giá trị thì thay toàn bộ (409 nếu Quiz đã có bài làm).</summary>
public sealed record UpdateQuizRequest(
    string? Title,
    string? DescriptionMarkdown,
    string? Status,
    IReadOnlyList<QuizQuestionRequest>? Questions);

public sealed record QuizAnswerRequest(Guid? QuestionId, IReadOnlyList<Guid>? OptionIds);

public sealed record SaveQuizAnswersRequest(IReadOnlyList<QuizAnswerRequest>? Answers);

/// <summary><see cref="Answers"/> null nghĩa là nộp đáp án nháp đã lưu.</summary>
public sealed record SubmitQuizAttemptRequest(IReadOnlyList<QuizAnswerRequest>? Answers);
