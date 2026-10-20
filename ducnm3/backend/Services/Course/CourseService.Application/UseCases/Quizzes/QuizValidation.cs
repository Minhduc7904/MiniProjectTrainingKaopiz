// File: backend/Services/Course/CourseService.Application/UseCases/Quizzes/QuizValidation.cs
// Mục đích: Normalize và validate Quiz, câu hỏi, lựa chọn dùng chung cho Create và Update.

using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace CourseService.Application.UseCases.Quizzes;

public static class QuizValidation
{
    public const int TitleMinLength = 2;
    public const int TitleMaxLength = 200;
    public const int MarkdownMaxLength = 20_000;
    public const int OptionMaxLength = 2_000;
    public const int MaxQuestions = 100;
    public const int MinOptions = 2;
    public const int MaxOptions = 10;
    public const decimal MaxPoints = 100m;

    /// <param name="existingQuestionCount">Số câu hiện có khi không gửi lại cây câu hỏi (update); dùng để kiểm tra điều kiện publish.</param>
    public static QuizDraft Normalize(QuizInput input, string? defaultStatus, int existingQuestionCount)
    {
        ArgumentNullException.ThrowIfNull(input);
        var details = new List<ApiErrorDetail>();

        var title = input.Title?.Trim();
        if (string.IsNullOrEmpty(title) || title.Length is < TitleMinLength or > TitleMaxLength)
        {
            details.Add(new ApiErrorDetail("title", $"Title must be between {TitleMinLength} and {TitleMaxLength} characters."));
        }

        var description = string.IsNullOrWhiteSpace(input.DescriptionMarkdown) ? null : input.DescriptionMarkdown;
        if (description is { Length: > MarkdownMaxLength })
        {
            details.Add(new ApiErrorDetail("descriptionMarkdown", $"Description must be at most {MarkdownMaxLength} characters."));
        }

        var status = input.Status?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(status)) status = defaultStatus;
        if (status is null || !QuizStatuses.IsSupported(status))
        {
            details.Add(new ApiErrorDetail("status", $"Status must be {QuizStatuses.Draft} or {QuizStatuses.Published}."));
        }

        var questions = input.Questions is null ? null : NormalizeQuestions(input.Questions, details);
        var questionCount = questions?.Count ?? existingQuestionCount;
        if (status == QuizStatuses.Published && questionCount == 0)
        {
            details.Add(new ApiErrorDetail("status", "A quiz needs at least one question before it can be published."));
        }

        if (details.Count > 0) throw CourseErrors.ValidationFailed(details);
        return new QuizDraft(title!, description, status!, questions);
    }

    private static List<QuizQuestionDraft> NormalizeQuestions(IReadOnlyList<QuizQuestionInput> inputs, List<ApiErrorDetail> details)
    {
        var result = new List<QuizQuestionDraft>(inputs.Count);
        if (inputs.Count > MaxQuestions)
        {
            details.Add(new ApiErrorDetail("questions", $"A quiz can have at most {MaxQuestions} questions."));
            return result;
        }

        for (var index = 0; index < inputs.Count; index++)
        {
            var path = $"questions[{index}]";
            var input = inputs[index];
            var markdown = input.QuestionMarkdown?.Trim();
            if (string.IsNullOrEmpty(markdown) || markdown.Length > MarkdownMaxLength)
            {
                details.Add(new ApiErrorDetail($"{path}.questionMarkdown", $"Question content is required and must be at most {MarkdownMaxLength} characters."));
            }

            var type = input.QuestionType?.Trim().ToUpperInvariant();
            if (type is null || !QuizQuestionTypes.IsSupported(type))
            {
                details.Add(new ApiErrorDetail($"{path}.questionType", $"Question type must be {QuizQuestionTypes.SingleChoice} or {QuizQuestionTypes.MultipleChoice}."));
            }

            var points = input.Points;
            if (points is not > 0m || points > MaxPoints || decimal.Round(points.Value, 2) != points)
            {
                details.Add(new ApiErrorDetail($"{path}.points", $"Points must be greater than 0, at most {MaxPoints} and have at most 2 decimal places."));
            }

            var options = NormalizeOptions(input.Options, path, type, details);
            if (markdown is not null && type is not null && points is > 0m)
            {
                result.Add(new QuizQuestionDraft(markdown, type, points.Value, options));
            }
        }

        return result;
    }

    private static List<QuizOptionDraft> NormalizeOptions(
        IReadOnlyList<QuizOptionInput>? inputs,
        string questionPath,
        string? type,
        List<ApiErrorDetail> details)
    {
        var result = new List<QuizOptionDraft>();
        if (inputs is null || inputs.Count is < MinOptions or > MaxOptions)
        {
            details.Add(new ApiErrorDetail($"{questionPath}.options", $"A question needs between {MinOptions} and {MaxOptions} options."));
            return result;
        }

        for (var index = 0; index < inputs.Count; index++)
        {
            var markdown = inputs[index].OptionMarkdown?.Trim();
            if (string.IsNullOrEmpty(markdown) || markdown.Length > OptionMaxLength)
            {
                details.Add(new ApiErrorDetail($"{questionPath}.options[{index}].optionMarkdown", $"Option content is required and must be at most {OptionMaxLength} characters."));
                continue;
            }

            result.Add(new QuizOptionDraft(markdown, inputs[index].IsCorrect ?? false));
        }

        var correct = result.Count(option => option.IsCorrect);
        if (type == QuizQuestionTypes.SingleChoice && correct != 1)
        {
            details.Add(new ApiErrorDetail($"{questionPath}.options", "A single-choice question needs exactly one correct option."));
        }
        else if (type == QuizQuestionTypes.MultipleChoice && correct < 1)
        {
            details.Add(new ApiErrorDetail($"{questionPath}.options", "A multiple-choice question needs at least one correct option."));
        }

        return result;
    }

    /// <summary>Gom toàn bộ Markdown của Quiz để đồng bộ media usage theo owner Quiz.</summary>
    public static string CombineMarkdown(string? description, IEnumerable<QuizQuestionDraft>? questions) =>
        string.Join("\n\n", new[] { description }.Concat(
            (questions ?? []).SelectMany(question => new[] { question.QuestionMarkdown }.Concat(question.Options.Select(option => option.OptionMarkdown))))
            .Where(text => !string.IsNullOrWhiteSpace(text)));

    public static string CombineMarkdown(QuizRecord quiz) =>
        string.Join("\n\n", new[] { quiz.DescriptionMarkdown }.Concat(
            quiz.Questions.SelectMany(question => new[] { question.QuestionMarkdown }.Concat(question.Options.Select(option => option.OptionMarkdown))))
            .Where(text => !string.IsNullOrWhiteSpace(text)));
}
