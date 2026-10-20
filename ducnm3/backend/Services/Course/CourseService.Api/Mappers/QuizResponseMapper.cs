// File: backend/Services/Course/CourseService.Api/Mappers/QuizResponseMapper.cs
// Mục đích: Chuyển record Quiz của Application thành HTTP contract, render Markdown thành HTML an toàn.

using CourseService.Api.Contracts.Quizzes;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Content;
using CourseService.Application.UseCases.Quizzes;

namespace CourseService.Api.Mappers;

public static class QuizResponseMapper
{
    public static QuizResponse ToResponse(QuizRecord quiz, IMarkdownHtmlRenderer renderer) =>
        new(
            quiz.Id,
            quiz.LessonId,
            quiz.Title,
            quiz.DescriptionMarkdown,
            renderer.Render(quiz.DescriptionMarkdown),
            quiz.DisplayOrder,
            quiz.Status,
            quiz.Questions.Count,
            quiz.TotalPoints,
            quiz.HasAttempts,
            quiz.CreatedAtUtc,
            quiz.UpdatedAtUtc,
            quiz.Questions.Select(question => new QuizQuestionResponse(
                question.Id,
                question.QuestionMarkdown,
                renderer.Render(question.QuestionMarkdown),
                question.QuestionType,
                question.Points,
                question.DisplayOrder,
                question.Options.Select(option => new QuizOptionResponse(
                    option.Id,
                    option.OptionMarkdown,
                    renderer.Render(option.OptionMarkdown),
                    option.IsCorrect,
                    option.DisplayOrder)).ToArray())).ToArray());

    public static IReadOnlyList<QuizSummaryResponse> ToResponse(IReadOnlyList<QuizSummaryRecord> quizzes, IMarkdownHtmlRenderer renderer) =>
        quizzes.Select(quiz => new QuizSummaryResponse(
            quiz.Id,
            quiz.LessonId,
            quiz.Title,
            quiz.DescriptionMarkdown,
            renderer.Render(quiz.DescriptionMarkdown),
            quiz.DisplayOrder,
            quiz.Status,
            quiz.QuestionCount,
            quiz.TotalPoints,
            quiz.HasAttempts,
            quiz.CreatedAtUtc,
            quiz.UpdatedAtUtc)).ToArray();

    public static StudentQuizResponse ToStudentResponse(QuizRecord quiz, IMarkdownHtmlRenderer renderer) =>
        new(
            quiz.Id,
            quiz.Title,
            renderer.Render(quiz.DescriptionMarkdown),
            quiz.DisplayOrder,
            quiz.Questions.Count,
            quiz.TotalPoints,
            quiz.Questions.Select(question => new StudentQuizQuestionResponse(
                question.Id,
                renderer.Render(question.QuestionMarkdown),
                question.QuestionType,
                question.Points,
                question.DisplayOrder,
                question.Options.Select(option => new StudentQuizOptionResponse(
                    option.Id,
                    renderer.Render(option.OptionMarkdown),
                    option.DisplayOrder)).ToArray())).ToArray());

    public static IReadOnlyList<StudentQuizSummaryResponse> ToStudentResponse(
        IReadOnlyList<StudentQuizSummaryRecord> quizzes,
        IMarkdownHtmlRenderer renderer) =>
        quizzes.Select(quiz => new StudentQuizSummaryResponse(
            quiz.Id,
            quiz.Title,
            renderer.Render(quiz.DescriptionMarkdown),
            quiz.DisplayOrder,
            quiz.QuestionCount,
            quiz.TotalPoints,
            quiz.BestScore,
            quiz.BestMaxScore,
            quiz.SubmittedAttempts,
            quiz.InProgressAttemptId)).ToArray();

    public static QuizAttemptResponse ToResponse(QuizAttemptRecord attempt) =>
        new(
            attempt.Id,
            attempt.QuizId,
            attempt.Status,
            attempt.StartedAtUtc,
            attempt.UpdatedAtUtc,
            attempt.SubmittedAtUtc,
            attempt.Score,
            attempt.MaxScore,
            attempt.Answers.Select(answer => answer.QuestionId).Distinct().Count(),
            attempt.Answers
                .GroupBy(answer => answer.QuestionId)
                .Select(group => new QuizAnswerResponse(group.Key, group.Select(answer => answer.OptionId).ToArray()))
                .ToArray());

    public static IReadOnlyList<QuizAttemptSummaryResponse> ToResponse(IReadOnlyList<QuizAttemptSummaryRecord> attempts) =>
        attempts.Select(attempt => new QuizAttemptSummaryResponse(
            attempt.Id,
            attempt.Status,
            attempt.StartedAtUtc,
            attempt.UpdatedAtUtc,
            attempt.SubmittedAtUtc,
            attempt.Score,
            attempt.MaxScore,
            attempt.AnsweredQuestions)).ToArray();

    public static QuizResultResponse ToResponse(QuizGrade grade) =>
        new(
            grade.Score,
            grade.MaxScore,
            grade.Questions.Count(question => question.Correct),
            grade.Questions.Count(question => question.Answered && !question.Correct),
            grade.Questions.Count(question => !question.Answered),
            grade.Questions.Select(question => new QuizQuestionResultResponse(
                question.QuestionId,
                question.Answered,
                question.Correct,
                question.PointsEarned,
                question.PointsPossible,
                question.SelectedOptionIds,
                question.CorrectOptionIds)).ToArray());

    public static QuizAttemptDetailResponse ToDetailResponse(QuizAttemptRecord attempt, QuizGrade? grade) =>
        new(ToResponse(attempt), grade is null ? null : ToResponse(grade));
}
