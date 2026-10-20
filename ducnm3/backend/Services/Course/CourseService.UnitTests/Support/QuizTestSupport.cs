using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Learning;
using CourseService.Application.UseCases.Quizzes;
using CourseService.Domain.Constants;

namespace CourseService.UnitTests.Support;

internal sealed class RecordingCommandSender : ICommandSender
{
    public List<ICommand> Commands { get; } = [];

    public Task SendAsync<TCommand>(string destinationService, TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
    {
        Commands.Add(command);
        return Task.CompletedTask;
    }
}

/// <summary>Kho Quiz trong bộ nhớ dùng chung cho cả port admin và port học viên, mô phỏng đúng hợp đồng của repository EF.</summary>
internal sealed class InMemoryQuizStore : IQuizCommandRepository, IStudentQuizRepository
{
    public Guid CourseId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid LessonId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public Dictionary<Guid, QuizRecord> Quizzes { get; } = [];

    public Dictionary<Guid, QuizAttemptRecord> Attempts { get; } = [];

    public Dictionary<Guid, Guid> AttemptOwners { get; } = [];

    public Task<bool> LessonExistsAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken) =>
        Task.FromResult(courseId == CourseId && lessonId == LessonId);

    public Task<IReadOnlyList<QuizSummaryRecord>> ListAsync(Guid lessonId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<QuizSummaryRecord>>(Quizzes.Values.Where(quiz => quiz.LessonId == lessonId)
            .OrderBy(quiz => quiz.DisplayOrder)
            .Select(quiz => new QuizSummaryRecord(
                quiz.Id, quiz.LessonId, quiz.Title, quiz.DescriptionMarkdown, quiz.DisplayOrder, quiz.Status,
                quiz.Questions.Count, quiz.TotalPoints, quiz.HasAttempts, quiz.CreatedAtUtc, quiz.UpdatedAtUtc))
            .ToArray());

    public Task<QuizRecord?> GetAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken) =>
        Task.FromResult(Quizzes.TryGetValue(quizId, out var quiz) && quiz.LessonId == lessonId
            ? quiz with { HasAttempts = Attempts.Values.Any(attempt => attempt.QuizId == quizId) }
            : null);

    public Task<QuizRecord> CreateAsync(Guid lessonId, QuizDraft draft, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var quiz = new QuizRecord(
            Guid.NewGuid(), lessonId, draft.Title, draft.DescriptionMarkdown,
            checked((uint)Quizzes.Values.Count(item => item.LessonId == lessonId) + 1), draft.Status, now, now, false,
            BuildQuestions(draft.Questions ?? []));
        Quizzes[quiz.Id] = quiz;
        return Task.FromResult(quiz);
    }

    public Task<QuizRecord?> UpdateAsync(Guid lessonId, Guid quizId, QuizDraft draft, CancellationToken cancellationToken)
    {
        if (!Quizzes.TryGetValue(quizId, out var current) || current.LessonId != lessonId) return Task.FromResult<QuizRecord?>(null);
        var updated = current with
        {
            Title = draft.Title,
            DescriptionMarkdown = draft.DescriptionMarkdown,
            Status = draft.Status,
            UpdatedAtUtc = DateTime.UtcNow,
            Questions = draft.Questions is null ? current.Questions : BuildQuestions(draft.Questions),
        };
        Quizzes[quizId] = updated;
        return Task.FromResult<QuizRecord?>(updated);
    }

    public Task<bool> HasAttemptsAsync(Guid quizId, CancellationToken cancellationToken) =>
        Task.FromResult(Attempts.Values.Any(attempt => attempt.QuizId == quizId));

    public Task<bool> DeleteAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken) =>
        Task.FromResult(Quizzes.TryGetValue(quizId, out var quiz) && quiz.LessonId == lessonId && Quizzes.Remove(quizId));

    public Task<IReadOnlyList<Guid>> GetQuizIdsByLessonAsync(Guid lessonId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Guid>>(Quizzes.Values.Where(quiz => quiz.LessonId == lessonId).Select(quiz => quiz.Id).ToArray());

    public Task<IReadOnlyList<Guid>> GetQuizIdsByCourseAsync(Guid courseId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Guid>>(courseId == CourseId ? Quizzes.Keys.ToArray() : []);

    public Task<IReadOnlyList<StudentQuizSummaryRecord>> ListPublishedAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<StudentQuizSummaryRecord>>(Quizzes.Values
            .Where(quiz => quiz.LessonId == lessonId && quiz.Status == QuizStatuses.Published)
            .Select(quiz =>
            {
                var own = Attempts.Values.Where(attempt => attempt.QuizId == quiz.Id && AttemptOwners[attempt.Id] == studentId).ToArray();
                var best = own.Where(attempt => attempt.Status == QuizAttemptStatuses.Submitted).OrderByDescending(attempt => attempt.Score).FirstOrDefault();
                return new StudentQuizSummaryRecord(
                    quiz.Id, quiz.Title, quiz.DescriptionMarkdown, quiz.DisplayOrder, quiz.Questions.Count, quiz.TotalPoints,
                    best?.Score, best?.MaxScore, own.Count(attempt => attempt.Status == QuizAttemptStatuses.Submitted),
                    own.FirstOrDefault(attempt => attempt.Status == QuizAttemptStatuses.InProgress)?.Id);
            }).ToArray());

    public Task<QuizRecord?> GetPublishedAsync(Guid lessonId, Guid quizId, CancellationToken cancellationToken) =>
        Task.FromResult(Quizzes.TryGetValue(quizId, out var quiz) && quiz.LessonId == lessonId && quiz.Status == QuizStatuses.Published ? quiz : null);

    public Task<QuizAttemptStartResult> StartAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken)
    {
        var open = Attempts.Values.FirstOrDefault(attempt =>
            attempt.QuizId == quizId && AttemptOwners[attempt.Id] == studentId && attempt.Status == QuizAttemptStatuses.InProgress);
        if (open is not null) return Task.FromResult(new QuizAttemptStartResult(open, false));

        var now = DateTime.UtcNow;
        var created = new QuizAttemptRecord(Guid.NewGuid(), quizId, QuizAttemptStatuses.InProgress, now, now, null, null, null, []);
        Attempts[created.Id] = created;
        AttemptOwners[created.Id] = studentId;
        return Task.FromResult(new QuizAttemptStartResult(created, true));
    }

    public Task<QuizAttemptRecord?> GetAttemptAsync(Guid quizId, Guid attemptId, Guid studentId, CancellationToken cancellationToken) =>
        Task.FromResult(Attempts.TryGetValue(attemptId, out var attempt) && attempt.QuizId == quizId && AttemptOwners[attemptId] == studentId ? attempt : null);

    public Task<IReadOnlyList<QuizAttemptSummaryRecord>> ListAttemptsAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<QuizAttemptSummaryRecord>>(Attempts.Values
            .Where(attempt => attempt.QuizId == quizId && AttemptOwners[attempt.Id] == studentId)
            .OrderByDescending(attempt => attempt.StartedAtUtc)
            .Select(attempt => new QuizAttemptSummaryRecord(
                attempt.Id, attempt.Status, attempt.StartedAtUtc, attempt.UpdatedAtUtc, attempt.SubmittedAtUtc, attempt.Score,
                attempt.MaxScore, attempt.Answers.Select(answer => answer.QuestionId).Distinct().Count()))
            .ToArray());

    public Task<QuizAttemptRecord?> SaveAnswersAsync(Guid attemptId, Guid studentId, IReadOnlyList<QuizAnswerSelection> answers, CancellationToken cancellationToken)
    {
        if (!Attempts.TryGetValue(attemptId, out var attempt) || AttemptOwners[attemptId] != studentId || attempt.Status != QuizAttemptStatuses.InProgress)
        {
            return Task.FromResult<QuizAttemptRecord?>(null);
        }

        var saved = attempt with { Answers = answers, UpdatedAtUtc = DateTime.UtcNow };
        Attempts[attemptId] = saved;
        return Task.FromResult<QuizAttemptRecord?>(saved);
    }

    public Task<QuizAttemptRecord?> SubmitAsync(
        Guid attemptId, Guid studentId, IReadOnlyList<QuizAnswerSelection>? finalAnswers, decimal score, decimal maxScore, CancellationToken cancellationToken)
    {
        if (!Attempts.TryGetValue(attemptId, out var attempt) || AttemptOwners[attemptId] != studentId || attempt.Status != QuizAttemptStatuses.InProgress)
        {
            return Task.FromResult<QuizAttemptRecord?>(null);
        }

        var now = DateTime.UtcNow;
        var submitted = attempt with
        {
            Status = QuizAttemptStatuses.Submitted,
            SubmittedAtUtc = now,
            UpdatedAtUtc = now,
            Score = score,
            MaxScore = maxScore,
            Answers = finalAnswers ?? attempt.Answers,
        };
        Attempts[attemptId] = submitted;
        return Task.FromResult<QuizAttemptRecord?>(submitted);
    }

    private static List<QuizQuestionRecord> BuildQuestions(IReadOnlyList<QuizQuestionDraft> drafts) =>
        drafts.Select((question, questionIndex) => new QuizQuestionRecord(
            Guid.NewGuid(),
            question.QuestionMarkdown,
            question.QuestionType,
            question.Points,
            checked((uint)questionIndex + 1),
            question.Options.Select((option, optionIndex) =>
                new QuizOptionRecord(Guid.NewGuid(), option.OptionMarkdown, option.IsCorrect, checked((uint)optionIndex + 1))).ToArray()))
            .ToList();
}

/// <summary>Chỉ hỗ trợ IsEnrolledAsync; các thao tác khác không dùng trong test Quiz.</summary>
internal sealed class StubEnrollment(Guid courseId, Guid studentId) : IStudentLearningRepository
{
    public Task<bool> IsEnrolledAsync(Guid id, Guid student, CancellationToken cancellationToken) =>
        Task.FromResult(id == courseId && student == studentId);

    public Task<StudentEnrollmentsResult> GetEnrollmentsAsync(Guid student, GetStudentEnrollmentsQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<StudentCourseCatalogResult> GetCatalogAsync(Guid student, GetStudentCourseCatalogQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<StudentCourseDetailResult?> GetDetailAsync(Guid id, Guid student, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<StudentCourseProgressResult?> GetProgressAsync(Guid id, Guid student, CancellationToken cancellationToken) => throw new NotSupportedException();
}

internal sealed class StubLessonRepository(Guid courseId, Guid lessonId) : ILessonCommandRepository
{
    public Task<LessonCreateRecord?> GetAsync(Guid course, Guid lesson, CancellationToken cancellationToken) =>
        Task.FromResult<LessonCreateRecord?>(course == courseId && lesson == lessonId
            ? new LessonCreateRecord(lessonId, courseId, "Lesson", null, 1, DateTime.UtcNow, DateTime.UtcNow)
            : null);

    public Task<LessonCreateRecord?> CreateAsync(Guid course, string title, string? contentMarkdown, uint? displayOrder, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<LessonCreateRecord?> UpdateAsync(Guid course, Guid lesson, string title, string? contentMarkdown, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> ReorderAsync(Guid course, IReadOnlyList<Guid> lessonIds, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> DeleteAsync(Guid course, Guid lesson, CancellationToken cancellationToken) => throw new NotSupportedException();
}

internal static class QuizBuilders
{
    public static QuizInput ValidInput(string status = QuizStatuses.Draft) =>
        new(
            "Kiểm tra cuối bài",
            "Mô tả quiz",
            status,
            [
                new("Thủ đô của Việt Nam là gì?", QuizQuestionTypes.SingleChoice, 2m,
                    [new("Hà Nội", true), new("Huế", false), new("Đà Nẵng", false)]),
                new("Chọn các số nguyên tố", QuizQuestionTypes.MultipleChoice, 3m,
                    [new("2", true), new("3", true), new("4", false), new("5", true)]),
            ]);
}
