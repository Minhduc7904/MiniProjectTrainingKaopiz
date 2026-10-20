// File: backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfStudentQuizRepository.cs
// Mục đích: Adapter EF Core cho học viên làm Quiz: bài làm dở, lưu nháp, nộp và xem lại.

using CourseService.Application.Repositories;
using CourseService.Domain.Constants;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfStudentQuizRepository(CourseDbContext db) : IStudentQuizRepository
{
    public async Task<IReadOnlyList<StudentQuizSummaryRecord>> ListPublishedAsync(
        Guid lessonId, Guid studentId, CancellationToken cancellationToken)
    {
        var quizzes = await db.Quizzes.AsNoTracking()
            .Where(item => item.LessonId == lessonId && item.Status == QuizStatuses.Published)
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new
            {
                item.Id,
                item.Title,
                item.DescriptionMarkdown,
                item.DisplayOrder,
                QuestionCount = item.QuizQuestions.Count,
                TotalPoints = item.QuizQuestions.Sum(question => (decimal?)question.Points) ?? 0m,
            })
            .ToListAsync(cancellationToken);
        var quizIds = quizzes.Select(item => item.Id).ToArray();
        var attempts = await db.QuizAttempts.AsNoTracking()
            .Where(item => item.StudentId == studentId && Enumerable.Contains(quizIds, item.QuizId))
            .Select(item => new { item.Id, item.QuizId, item.Status, item.Score, item.MaxScore })
            .ToListAsync(cancellationToken);

        return quizzes.Select(quiz =>
        {
            var own = attempts.Where(attempt => attempt.QuizId == quiz.Id).ToArray();
            var best = own.Where(attempt => attempt.Status == QuizAttemptStatuses.Submitted)
                .OrderByDescending(attempt => attempt.Score).FirstOrDefault();
            var inProgress = own.FirstOrDefault(attempt => attempt.Status == QuizAttemptStatuses.InProgress);
            return new StudentQuizSummaryRecord(
                quiz.Id,
                quiz.Title,
                quiz.DescriptionMarkdown,
                quiz.DisplayOrder,
                quiz.QuestionCount,
                quiz.TotalPoints,
                best?.Score,
                best?.MaxScore,
                own.Count(attempt => attempt.Status == QuizAttemptStatuses.Submitted),
                inProgress?.Id);
        }).ToArray();
    }

    public Task<QuizRecord?> GetPublishedAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken) =>
        QuizRecordMapper.ReadTreeAsync(
            db,
            db.Quizzes.Where(item => item.LessonId == lessonId && item.Id == quizId && item.Status == QuizStatuses.Published),
            cancellationToken);

    public async Task<QuizAttemptStartResult> StartAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken)
    {
        var existing = await FindInProgressAsync(quizId, studentId, cancellationToken);
        if (existing is not null) return new QuizAttemptStartResult(existing, false);

        var now = DateTime.UtcNow;
        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            StudentId = studentId,
            Status = QuizAttemptStatuses.InProgress,
            StartedAt = now,
            UpdatedAt = now,
        };
        db.QuizAttempts.Add(attempt);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsDuplicateKey())
        {
            // Hai request Start cùng lúc: unique open_marker giữ lại một bài làm dở, trả lại bài đó.
            db.ChangeTracker.Clear();
            var winner = await FindInProgressAsync(quizId, studentId, cancellationToken);
            if (winner is null) throw;
            return new QuizAttemptStartResult(winner, false);
        }

        return new QuizAttemptStartResult(
            new QuizAttemptRecord(attempt.Id, attempt.QuizId, attempt.Status, attempt.StartedAt, attempt.UpdatedAt, null, null, null, []),
            true);
    }

    public async Task<QuizAttemptRecord?> GetAttemptAsync(
        Guid quizId, Guid attemptId, Guid studentId, CancellationToken cancellationToken)
    {
        var attempt = await db.QuizAttempts.AsNoTracking()
            .Include(item => item.QuizAttemptAnswers)
            .SingleOrDefaultAsync(item => item.Id == attemptId && item.QuizId == quizId && item.StudentId == studentId, cancellationToken);
        return attempt is null ? null : ToRecord(attempt);
    }

    public async Task<IReadOnlyList<QuizAttemptSummaryRecord>> ListAttemptsAsync(
        Guid quizId, Guid studentId, CancellationToken cancellationToken) =>
        await db.QuizAttempts.AsNoTracking()
            .Where(item => item.QuizId == quizId && item.StudentId == studentId)
            .OrderByDescending(item => item.StartedAt)
            .Select(item => new QuizAttemptSummaryRecord(
                item.Id,
                item.Status,
                item.StartedAt,
                item.UpdatedAt,
                item.SubmittedAt,
                item.Score,
                item.MaxScore,
                item.QuizAttemptAnswers.Select(answer => answer.QuestionId).Distinct().Count()))
            .ToListAsync(cancellationToken);

    public async Task<QuizAttemptRecord?> SaveAnswersAsync(
        Guid attemptId, Guid studentId, IReadOnlyList<QuizAnswerSelection> answers, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // UPDATE có điều kiện status khóa dòng attempt, tuần tự hóa với Submit chạy cùng lúc.
        var touched = await db.QuizAttempts
            .Where(item => item.Id == attemptId && item.StudentId == studentId && item.Status == QuizAttemptStatuses.InProgress)
            .ExecuteUpdateAsync(set => set.SetProperty(item => item.UpdatedAt, DateTime.UtcNow), cancellationToken);
        if (touched == 0) return null;

        await ReplaceAnswersAsync(attemptId, answers, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        db.ChangeTracker.Clear();
        return await ReadAttemptAsync(attemptId, cancellationToken);
    }

    public async Task<QuizAttemptRecord?> SubmitAsync(
        Guid attemptId,
        Guid studentId,
        IReadOnlyList<QuizAnswerSelection>? finalAnswers,
        decimal score,
        decimal maxScore,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var submitted = await db.QuizAttempts
            .Where(item => item.Id == attemptId && item.StudentId == studentId && item.Status == QuizAttemptStatuses.InProgress)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(item => item.Status, QuizAttemptStatuses.Submitted)
                    .SetProperty(item => item.SubmittedAt, now)
                    .SetProperty(item => item.UpdatedAt, now)
                    .SetProperty(item => item.Score, score)
                    .SetProperty(item => item.MaxScore, maxScore),
                cancellationToken);
        if (submitted == 0) return null;

        if (finalAnswers is not null)
        {
            await ReplaceAnswersAsync(attemptId, finalAnswers, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        db.ChangeTracker.Clear();
        return await ReadAttemptAsync(attemptId, cancellationToken);
    }

    private async Task<QuizAttemptRecord?> FindInProgressAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken)
    {
        var attempt = await db.QuizAttempts.AsNoTracking()
            .Include(item => item.QuizAttemptAnswers)
            .SingleOrDefaultAsync(
                item => item.QuizId == quizId && item.StudentId == studentId && item.Status == QuizAttemptStatuses.InProgress,
                cancellationToken);
        return attempt is null ? null : ToRecord(attempt);
    }

    private async Task<QuizAttemptRecord?> ReadAttemptAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await db.QuizAttempts.AsNoTracking()
            .Include(item => item.QuizAttemptAnswers)
            .SingleOrDefaultAsync(item => item.Id == attemptId, cancellationToken);
        return attempt is null ? null : ToRecord(attempt);
    }

    private async Task ReplaceAnswersAsync(Guid attemptId, IReadOnlyList<QuizAnswerSelection> answers, CancellationToken cancellationToken)
    {
        await db.QuizAttemptAnswers.Where(item => item.AttemptId == attemptId).ExecuteDeleteAsync(cancellationToken);
        if (answers.Count == 0) return;
        db.QuizAttemptAnswers.AddRange(answers.Select(answer => new QuizAttemptAnswer
        {
            Id = Guid.NewGuid(),
            AttemptId = attemptId,
            QuestionId = answer.QuestionId,
            SelectedOptionId = answer.OptionId,
        }));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static QuizAttemptRecord ToRecord(QuizAttempt attempt) =>
        new(
            attempt.Id,
            attempt.QuizId,
            attempt.Status,
            attempt.StartedAt,
            attempt.UpdatedAt,
            attempt.SubmittedAt,
            attempt.Score,
            attempt.MaxScore,
            attempt.QuizAttemptAnswers.Select(answer => new QuizAnswerSelection(answer.QuestionId, answer.SelectedOptionId)).ToArray());
}
