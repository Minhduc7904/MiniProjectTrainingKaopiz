// File: backend/Services/Course/CourseService.Application/UseCases/Quizzes/QuizInput.cs
// Mục đích: Input thô (chưa validate) của Quiz từ API, trước khi QuizValidation normalize.

namespace CourseService.Application.UseCases.Quizzes;

public sealed record QuizOptionInput(string? OptionMarkdown, bool? IsCorrect);

public sealed record QuizQuestionInput(
    string? QuestionMarkdown,
    string? QuestionType,
    decimal? Points,
    IReadOnlyList<QuizOptionInput>? Options);

/// <summary><see cref="Questions"/> null khi cập nhật nghĩa là giữ nguyên cây câu hỏi hiện có.</summary>
public sealed record QuizInput(
    string? Title,
    string? DescriptionMarkdown,
    string? Status,
    IReadOnlyList<QuizQuestionInput>? Questions);
