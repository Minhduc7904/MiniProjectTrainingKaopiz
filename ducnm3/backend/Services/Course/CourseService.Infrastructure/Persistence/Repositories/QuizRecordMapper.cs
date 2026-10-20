// File: backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/QuizRecordMapper.cs
// Mục đích: Đọc cây Quiz → Question → Option và đổi sang record Application, dùng chung cho admin và học viên.

using CourseService.Application.Repositories;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

internal static class QuizRecordMapper
{
    public static async Task<QuizRecord?> ReadTreeAsync(
        CourseDbContext db,
        IQueryable<Quiz> quizzes,
        CancellationToken cancellationToken)
    {
        var quiz = await quizzes
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.QuizQuestions)
            .ThenInclude(question => question.QuizOptions)
            .SingleOrDefaultAsync(cancellationToken);
        if (quiz is null) return null;
        var hasAttempts = await db.QuizAttempts.AsNoTracking().AnyAsync(item => item.QuizId == quiz.Id, cancellationToken);
        return ToRecord(quiz, hasAttempts);
    }

    public static QuizRecord ToRecord(Quiz quiz, bool hasAttempts) =>
        new(
            quiz.Id,
            quiz.LessonId,
            quiz.Title,
            quiz.DescriptionMarkdown,
            quiz.DisplayOrder,
            quiz.Status,
            quiz.CreatedAt,
            quiz.UpdatedAt,
            hasAttempts,
            quiz.QuizQuestions
                .OrderBy(question => question.DisplayOrder)
                .Select(question => new QuizQuestionRecord(
                    question.Id,
                    question.QuestionMarkdown,
                    question.QuestionType,
                    question.Points,
                    question.DisplayOrder,
                    question.QuizOptions
                        .OrderBy(option => option.DisplayOrder)
                        .Select(option => new QuizOptionRecord(option.Id, option.OptionMarkdown, option.IsCorrect, option.DisplayOrder))
                        .ToArray()))
                .ToArray());

    public static IReadOnlyList<QuizQuestion> BuildQuestions(Guid quizId, IReadOnlyList<QuizQuestionDraft> drafts)
    {
        var questions = new List<QuizQuestion>(drafts.Count);
        for (var questionIndex = 0; questionIndex < drafts.Count; questionIndex++)
        {
            var draft = drafts[questionIndex];
            var question = new QuizQuestion
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                QuestionMarkdown = draft.QuestionMarkdown,
                QuestionType = draft.QuestionType,
                Points = draft.Points,
                DisplayOrder = checked((uint)questionIndex + 1),
            };
            for (var optionIndex = 0; optionIndex < draft.Options.Count; optionIndex++)
            {
                question.QuizOptions.Add(new QuizOption
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    OptionMarkdown = draft.Options[optionIndex].OptionMarkdown,
                    IsCorrect = draft.Options[optionIndex].IsCorrect,
                    DisplayOrder = checked((uint)optionIndex + 1),
                });
            }

            questions.Add(question);
        }

        return questions;
    }
}
