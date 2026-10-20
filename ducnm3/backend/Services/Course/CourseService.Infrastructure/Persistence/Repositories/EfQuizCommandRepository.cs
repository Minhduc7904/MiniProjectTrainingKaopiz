// File: backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfQuizCommandRepository.cs
// Mục đích: Adapter EF Core cho soạn Quiz của admin; ghi cả cây câu hỏi trong một transaction.

using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfQuizCommandRepository(CourseDbContext db) : IQuizCommandRepository
{
    private const int MaxOrderAttempts = 3;

    public Task<bool> LessonExistsAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken) =>
        db.Lessons.AsNoTracking().AnyAsync(item => item.Id == lessonId && item.CourseId == courseId, cancellationToken);

    public async Task<IReadOnlyList<QuizSummaryRecord>> ListAsync(Guid lessonId, CancellationToken cancellationToken) =>
        await db.Quizzes.AsNoTracking()
            .Where(item => item.LessonId == lessonId)
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new QuizSummaryRecord(
                item.Id,
                item.LessonId,
                item.Title,
                item.DescriptionMarkdown,
                item.DisplayOrder,
                item.Status,
                item.QuizQuestions.Count,
                item.QuizQuestions.Sum(question => (decimal?)question.Points) ?? 0m,
                item.QuizAttempts.Any(),
                item.CreatedAt,
                item.UpdatedAt))
            .ToListAsync(cancellationToken);

    public Task<QuizRecord?> GetAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken) =>
        QuizRecordMapper.ReadTreeAsync(db, db.Quizzes.Where(item => item.LessonId == lessonId && item.Id == quizId), cancellationToken);

    public async Task<QuizRecord> CreateAsync(Guid lessonId, QuizDraft draft, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var nextOrder = (await db.Quizzes.Where(item => item.LessonId == lessonId)
                .MaxAsync(item => (uint?)item.DisplayOrder, cancellationToken) ?? 0u) + 1;
            var now = DateTime.UtcNow;
            var quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                LessonId = lessonId,
                Title = draft.Title,
                DescriptionMarkdown = draft.DescriptionMarkdown,
                DisplayOrder = nextOrder,
                Status = draft.Status,
                CreatedAt = now,
                UpdatedAt = now,
            };
            foreach (var question in QuizRecordMapper.BuildQuestions(quiz.Id, draft.Questions ?? []))
            {
                quiz.QuizQuestions.Add(question);
            }

            db.Quizzes.Add(quiz);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return QuizRecordMapper.ToRecord(quiz, hasAttempts: false);
            }
            catch (DbUpdateException exception) when (exception.IsDuplicateKey() && attempt < MaxOrderAttempts)
            {
                db.ChangeTracker.Clear();
            }
        }
    }

    public async Task<QuizRecord?> UpdateAsync(Guid lessonId, Guid quizId, QuizDraft draft, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var quiz = await db.Quizzes.SingleOrDefaultAsync(item => item.LessonId == lessonId && item.Id == quizId, cancellationToken);
        if (quiz is null) return null;

        quiz.Title = draft.Title;
        quiz.DescriptionMarkdown = draft.DescriptionMarkdown;
        quiz.Status = draft.Status;
        quiz.UpdatedAt = DateTime.UtcNow;
        if (draft.Questions is not null)
        {
            if (await db.QuizAttempts.AnyAsync(item => item.QuizId == quizId, cancellationToken))
            {
                throw CourseErrors.QuizHasAttempts();
            }

            await db.QuizQuestions.Where(item => item.QuizId == quizId).ExecuteDeleteAsync(cancellationToken);
            db.QuizQuestions.AddRange(QuizRecordMapper.BuildQuestions(quizId, draft.Questions));
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        db.ChangeTracker.Clear();
        return await GetAsync(lessonId, quizId, cancellationToken);
    }

    public Task<bool> HasAttemptsAsync(Guid quizId, CancellationToken cancellationToken) =>
        db.QuizAttempts.AsNoTracking().AnyAsync(item => item.QuizId == quizId, cancellationToken);

    public async Task<bool> DeleteAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken) =>
        await db.Quizzes.Where(item => item.LessonId == lessonId && item.Id == quizId).ExecuteDeleteAsync(cancellationToken) > 0;

    public async Task<IReadOnlyList<Guid>> GetQuizIdsByLessonAsync(Guid lessonId, CancellationToken cancellationToken) =>
        await db.Quizzes.AsNoTracking().Where(item => item.LessonId == lessonId).Select(item => item.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetQuizIdsByCourseAsync(Guid courseId, CancellationToken cancellationToken) =>
        await db.Quizzes.AsNoTracking().Where(item => item.Lesson.CourseId == courseId).Select(item => item.Id).ToListAsync(cancellationToken);
}
