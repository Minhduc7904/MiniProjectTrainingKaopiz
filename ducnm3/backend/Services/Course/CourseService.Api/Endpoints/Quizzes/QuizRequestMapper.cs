// File: backend/Services/Course/CourseService.Api/Endpoints/Quizzes/QuizRequestMapper.cs
// Mục đích: Chuyển request body Quiz của API thành input Application.

using CourseService.Api.Contracts.Quizzes;
using CourseService.Application.UseCases.Learning.Quizzes;
using CourseService.Application.UseCases.Quizzes;

namespace CourseService.Api.Endpoints.Quizzes;

internal static class QuizRequestMapper
{
    public static QuizInput ToInput(
        string? title,
        string? descriptionMarkdown,
        string? status,
        IReadOnlyList<QuizQuestionRequest>? questions) =>
        new(
            title,
            descriptionMarkdown,
            status,
            questions?.Select(question => new QuizQuestionInput(
                question.QuestionMarkdown,
                question.QuestionType,
                question.Points,
                question.Options?.Select(option => new QuizOptionInput(option.OptionMarkdown, option.IsCorrect)).ToArray())).ToArray());

    public static IReadOnlyList<QuizAnswerInput>? ToAnswers(IReadOnlyList<QuizAnswerRequest>? answers) =>
        answers?.Select(answer => new QuizAnswerInput(answer.QuestionId, answer.OptionIds)).ToArray();
}
