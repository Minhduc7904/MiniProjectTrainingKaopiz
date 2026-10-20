// File: backend/Services/Course/CourseService.Application/UseCases/Quizzes/QuizGrader.cs
// Mục đích: Chấm điểm Quiz bằng hàm thuần: câu đạt khi tập đáp án chọn bằng đúng tập đáp án đúng.

using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Quizzes;

public sealed record QuizQuestionGrade(
    Guid QuestionId,
    bool Answered,
    bool Correct,
    decimal PointsEarned,
    decimal PointsPossible,
    IReadOnlyList<Guid> SelectedOptionIds,
    IReadOnlyList<Guid> CorrectOptionIds);

public sealed record QuizGrade(decimal Score, decimal MaxScore, IReadOnlyList<QuizQuestionGrade> Questions);

public static class QuizGrader
{
    public static QuizGrade Grade(QuizRecord quiz, IReadOnlyList<QuizAnswerSelection> answers)
    {
        ArgumentNullException.ThrowIfNull(quiz);
        ArgumentNullException.ThrowIfNull(answers);
        var selectedByQuestion = answers
            .GroupBy(answer => answer.QuestionId)
            .ToDictionary(group => group.Key, group => group.Select(answer => answer.OptionId).Distinct().ToHashSet());

        var grades = new List<QuizQuestionGrade>(quiz.Questions.Count);
        foreach (var question in quiz.Questions)
        {
            var correctIds = question.Options.Where(option => option.IsCorrect).Select(option => option.Id).ToHashSet();
            selectedByQuestion.TryGetValue(question.Id, out var selected);
            selected ??= [];
            var correct = selected.Count > 0 && selected.SetEquals(correctIds);
            grades.Add(new QuizQuestionGrade(
                question.Id,
                selected.Count > 0,
                correct,
                correct ? question.Points : 0m,
                question.Points,
                question.Options.Where(option => selected.Contains(option.Id)).Select(option => option.Id).ToArray(),
                question.Options.Where(option => option.IsCorrect).Select(option => option.Id).ToArray()));
        }

        return new QuizGrade(grades.Sum(grade => grade.PointsEarned), grades.Sum(grade => grade.PointsPossible), grades);
    }
}
