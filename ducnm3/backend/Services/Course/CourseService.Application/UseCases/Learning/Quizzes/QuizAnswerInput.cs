// File: backend/Services/Course/CourseService.Application/UseCases/Learning/Quizzes/QuizAnswerInput.cs
// Mục đích: Input đáp án thô của học viên và validate chúng theo cấu trúc Quiz.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.Learning.Quizzes;

public sealed record QuizAnswerInput(Guid? QuestionId, IReadOnlyList<Guid>? OptionIds);

public static class QuizAnswerValidation
{
    /// <summary>Đáp án rỗng hợp lệ (câu bỏ trống); mỗi câu chỉ xuất hiện một lần và chỉ chứa lựa chọn thuộc câu đó.</summary>
    public static IReadOnlyList<QuizAnswerSelection> Normalize(QuizRecord quiz, IReadOnlyList<QuizAnswerInput>? inputs)
    {
        ArgumentNullException.ThrowIfNull(quiz);
        var result = new List<QuizAnswerSelection>();
        if (inputs is null) return result;

        var details = new List<ApiErrorDetail>();
        var questions = quiz.Questions.ToDictionary(question => question.Id);
        var seenQuestions = new HashSet<Guid>();
        for (var index = 0; index < inputs.Count; index++)
        {
            var path = $"answers[{index}]";
            var input = inputs[index];
            if (input.QuestionId is not { } questionId || !questions.TryGetValue(questionId, out var question))
            {
                details.Add(new ApiErrorDetail($"{path}.questionId", "Question does not belong to this quiz."));
                continue;
            }

            if (!seenQuestions.Add(questionId))
            {
                details.Add(new ApiErrorDetail($"{path}.questionId", "Question appears more than once."));
                continue;
            }

            var optionIds = (input.OptionIds ?? []).Distinct().ToArray();
            if (optionIds.Any(optionId => question.Options.All(option => option.Id != optionId)))
            {
                details.Add(new ApiErrorDetail($"{path}.optionIds", "Option does not belong to the question."));
                continue;
            }

            if (question.QuestionType == QuizQuestionTypes.SingleChoice && optionIds.Length > 1)
            {
                details.Add(new ApiErrorDetail($"{path}.optionIds", "A single-choice question accepts only one option."));
                continue;
            }

            result.AddRange(optionIds.Select(optionId => new QuizAnswerSelection(questionId, optionId)));
        }

        if (details.Count > 0) throw CourseErrors.ValidationFailed(details);
        return result;
    }
}
