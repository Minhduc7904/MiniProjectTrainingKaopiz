using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Learning.Quizzes;
using CourseService.Application.UseCases.Quizzes;
using CourseService.Domain.Constants;
using CourseService.UnitTests.Support;

namespace CourseService.UnitTests.Application.Quizzes;

public sealed class StudentQuizHandlersTests
{
    private static readonly Guid StudentId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    private InMemoryQuizStore _store = null!;
    private StudentQuizAccess _access = null!;
    private QuizRecord _quiz = null!;

    [SetUp]
    public async Task SetUp()
    {
        _store = new InMemoryQuizStore();
        _access = new StudentQuizAccess(new StubEnrollment(_store.CourseId, StudentId), new StubLessonRepository(_store.CourseId, _store.LessonId));
        _quiz = await _store.CreateAsync(_store.LessonId, Draft(QuizStatuses.Published), CancellationToken.None);
    }

    private static QuizDraft Draft(string status) =>
        new("Quiz", null, status,
        [
            new("Chọn 1", QuizQuestionTypes.SingleChoice, 2m, [new("Đúng", true), new("Sai", false)]),
            new("Chọn nhiều", QuizQuestionTypes.MultipleChoice, 3m, [new("X", true), new("Y", true), new("Z", false)]),
        ]);

    private QuizAnswerInput Answer(int question, params int[] options) =>
        new(_quiz.Questions[question].Id, options.Select(index => _quiz.Questions[question].Options[index].Id).ToArray());

    private static CourseApplicationException Fails(AsyncTestDelegate action) => Assert.ThrowsAsync<CourseApplicationException>(action)!;

    [Test]
    public async Task Start_FirstTime_CreatesAttempt()
    {
        var result = await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default);

        Assert.Multiple(() =>
        {
            Assert.That(result.Created, Is.True);
            Assert.That(result.Attempt.Status, Is.EqualTo(QuizAttemptStatuses.InProgress));
            Assert.That(result.Attempt.Answers, Is.Empty);
        });
    }

    [Test]
    public async Task Start_WithOpenAttempt_ResumesSavedAnswers()
    {
        var start = new StartQuizAttemptHandler(_access, _store);
        var first = await start.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default);
        await new SaveQuizAnswersHandler(_access, _store).HandleAsync(
            _store.CourseId, _store.LessonId, _quiz.Id, first.Attempt.Id, StudentId, [Answer(0, 1)], default);

        var resumed = await start.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default);

        Assert.Multiple(() =>
        {
            Assert.That(resumed.Created, Is.False);
            Assert.That(resumed.Attempt.Id, Is.EqualTo(first.Attempt.Id));
            Assert.That(resumed.Attempt.Answers.Single().OptionId, Is.EqualTo(_quiz.Questions[0].Options[1].Id));
        });
    }

    [Test]
    public async Task Start_AfterSubmit_CreatesNewAttemptAndKeepsOldOne()
    {
        var start = new StartQuizAttemptHandler(_access, _store);
        var first = await start.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default);
        await new SubmitQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, first.Attempt.Id, StudentId, null, default);

        var second = await start.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default);
        var attempts = await new GetQuizAttemptsHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default);

        Assert.Multiple(() =>
        {
            Assert.That(second.Created, Is.True);
            Assert.That(second.Attempt.Id, Is.Not.EqualTo(first.Attempt.Id));
            Assert.That(attempts, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Start_NotEnrolled_ThrowsStudentNotEnrolled()
    {
        var exception = Fails(() => new StartQuizAttemptHandler(_access, _store)
            .HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, Guid.NewGuid(), default));

        Assert.That(exception.ErrorCode, Is.EqualTo(CourseErrorCodes.StudentNotEnrolled));
    }

    [Test]
    public async Task Start_DraftQuiz_ThrowsQuizNotFound()
    {
        var draft = await _store.CreateAsync(_store.LessonId, Draft(QuizStatuses.Draft), default);

        var exception = Fails(() => new StartQuizAttemptHandler(_access, _store)
            .HandleAsync(_store.CourseId, _store.LessonId, draft.Id, StudentId, default));

        Assert.That(exception.ErrorCode, Is.EqualTo(CourseErrorCodes.QuizNotFound));
    }

    [Test]
    public async Task SaveAnswers_ReplacesPreviousDraftCompletely()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;
        var save = new SaveQuizAnswersHandler(_access, _store);
        await save.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [Answer(0, 0), Answer(1, 0, 1)], default);

        var saved = await save.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [Answer(1, 2)], default);

        Assert.That(saved.Answers.Select(answer => answer.OptionId), Is.EqualTo(new[] { _quiz.Questions[1].Options[2].Id }));
    }

    [Test]
    public async Task SaveAnswers_EmptyList_ClearsDraftWithoutGrading()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;

        var saved = await new SaveQuizAnswersHandler(_access, _store)
            .HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [], default);

        Assert.Multiple(() =>
        {
            Assert.That(saved.Answers, Is.Empty);
            Assert.That(saved.Score, Is.Null);
            Assert.That(saved.Status, Is.EqualTo(QuizAttemptStatuses.InProgress));
        });
    }

    [Test]
    public async Task SaveAnswers_ForeignOption_ThrowsValidationFailed()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;

        var exception = Fails(() => new SaveQuizAnswersHandler(_access, _store).HandleAsync(
            _store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId,
            [new QuizAnswerInput(_quiz.Questions[0].Id, [Guid.NewGuid()])], default));

        Assert.That(exception.ErrorCode, Is.EqualTo(ApiErrorCodes.ValidationFailed));
    }

    [Test]
    public async Task SaveAnswers_TwoOptionsOnSingleChoice_ThrowsValidationFailed()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;

        var exception = Fails(() => new SaveQuizAnswersHandler(_access, _store).HandleAsync(
            _store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [Answer(0, 0, 1)], default));

        Assert.That(exception.ErrorCode, Is.EqualTo(ApiErrorCodes.ValidationFailed));
    }

    [Test]
    public async Task SaveAnswers_SameQuestionTwice_ThrowsValidationFailed()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;

        var exception = Fails(() => new SaveQuizAnswersHandler(_access, _store).HandleAsync(
            _store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [Answer(0, 0), Answer(0, 1)], default));

        Assert.That(exception.ErrorCode, Is.EqualTo(ApiErrorCodes.ValidationFailed));
    }

    [Test]
    public async Task SaveAnswers_AfterSubmit_ThrowsAlreadySubmitted()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;
        await new SubmitQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, null, default);

        var exception = Fails(() => new SaveQuizAnswersHandler(_access, _store)
            .HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [], default));

        Assert.That(exception.ErrorCode, Is.EqualTo(CourseErrorCodes.QuizAttemptAlreadySubmitted));
    }

    [Test]
    public async Task Submit_UsesSavedDraftWhenNoFinalAnswers()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;
        await new SaveQuizAnswersHandler(_access, _store)
            .HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [Answer(0, 0), Answer(1, 0, 1)], default);

        var result = await new SubmitQuizAttemptHandler(_access, _store)
            .HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, null, default);

        Assert.Multiple(() =>
        {
            Assert.That(result.Grade.Score, Is.EqualTo(5m));
            Assert.That(result.Attempt.Status, Is.EqualTo(QuizAttemptStatuses.Submitted));
            Assert.That(result.Attempt.Score, Is.EqualTo(5m));
            Assert.That(result.Attempt.MaxScore, Is.EqualTo(5m));
        });
    }

    [Test]
    public async Task Submit_FinalAnswersOverrideDraft_AndMultipleChoiceIsAllOrNothing()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;

        var result = await new SubmitQuizAttemptHandler(_access, _store).HandleAsync(
            _store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [Answer(0, 0), Answer(1, 0)], default);

        Assert.Multiple(() =>
        {
            Assert.That(result.Grade.Score, Is.EqualTo(2m));
            Assert.That(result.Grade.MaxScore, Is.EqualTo(5m));
        });
    }

    [Test]
    public async Task Submit_Twice_ThrowsAlreadySubmitted()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;
        var submit = new SubmitQuizAttemptHandler(_access, _store);
        await submit.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, null, default);

        var exception = Fails(() => submit.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, null, default));

        Assert.That(exception.ErrorCode, Is.EqualTo(CourseErrorCodes.QuizAttemptAlreadySubmitted));
    }

    [Test]
    public async Task Attempt_OfAnotherStudent_IsNotFound()
    {
        var other = Guid.NewGuid();
        var otherAccess = new StudentQuizAccess(new StubEnrollment(_store.CourseId, other), new StubLessonRepository(_store.CourseId, _store.LessonId));
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;

        var exception = Fails(() => new GetQuizAttemptDetailHandler(otherAccess, _store)
            .HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, other, default));

        Assert.That(exception.ErrorCode, Is.EqualTo(CourseErrorCodes.QuizAttemptNotFound));
    }

    [Test]
    public async Task AttemptDetail_InProgress_HasNoGrade_AndSubmittedHasGrade()
    {
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;
        var detail = new GetQuizAttemptDetailHandler(_access, _store);

        var draft = await detail.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, default);
        await new SubmitQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [Answer(0, 0)], default);
        var submitted = await detail.HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, default);

        Assert.Multiple(() =>
        {
            Assert.That(draft.Grade, Is.Null);
            Assert.That(submitted.Grade!.Score, Is.EqualTo(2m));
        });
    }

    [Test]
    public async Task List_ShowsOnlyPublishedQuizzesWithBestScoreAndOpenAttempt()
    {
        await _store.CreateAsync(_store.LessonId, Draft(QuizStatuses.Draft), default);
        var attempt = (await new StartQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, StudentId, default)).Attempt;

        var open = (await new GetStudentQuizzesHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, StudentId, default)).Single();
        await new SubmitQuizAttemptHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, _quiz.Id, attempt.Id, StudentId, [Answer(0, 0)], default);
        var done = (await new GetStudentQuizzesHandler(_access, _store).HandleAsync(_store.CourseId, _store.LessonId, StudentId, default)).Single();

        Assert.Multiple(() =>
        {
            Assert.That(open.InProgressAttemptId, Is.EqualTo(attempt.Id));
            Assert.That(open.BestScore, Is.Null);
            Assert.That(done.InProgressAttemptId, Is.Null);
            Assert.That(done.BestScore, Is.EqualTo(2m));
            Assert.That(done.SubmittedAttempts, Is.EqualTo(1));
        });
    }
}
