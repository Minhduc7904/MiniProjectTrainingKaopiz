using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.LessonAi.Documents;
using CourseService.Application.UseCases.LessonAi.Jobs;
using CourseService.Domain.Constants;
using CourseService.UnitTests.Support;

namespace CourseService.UnitTests.Application.LessonAi;

public sealed class LessonAiHandlersTests
{
    private static readonly Guid Admin = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LessonId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private InMemoryLessonAiStore _store = null!;
    private FakeCourseMediaReader _media = null!;
    private MutableLessonRepository _lessons = null!;

    [SetUp]
    public void SetUp()
    {
        _store = new InMemoryLessonAiStore();
        _media = new FakeCourseMediaReader();
        _lessons = new MutableLessonRepository(CourseId, LessonId);
    }

    private SyncLessonAiDocumentsHandler Sync => new(_lessons, _media, _store);

    private static CreateLessonAiJobCommand JobCommand(string? type, IReadOnlyList<Guid>? mediaIds, int? questions = null, bool overwrite = false) =>
        new(CourseId, LessonId, Admin, type, mediaIds, questions, overwrite);

    private Guid ExtractedDocument() => _store.AddExtracted(LessonId, "bai.txt", "nội dung").MediaId;

    // ---- Sync / list documents ----

    [Test]
    public async Task SyncQueuesSupportedAttachmentsAndMarksOtherFilesUnsupported()
    {
        var pdf = _media.Add(LessonId, "bai.pdf", "application/pdf");
        var video = _media.Add(LessonId, "clip.mp4", "video/mp4", order: 2);

        var documents = await Sync.HandleAsync(new SyncLessonAiDocumentsCommand(CourseId, LessonId, Admin, false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(documents.Single(document => document.MediaId == pdf.MediaId).Status, Is.EqualTo(LessonAiDocumentStatuses.Pending));
            var unsupported = documents.Single(document => document.MediaId == video.MediaId);
            Assert.That(unsupported.Status, Is.EqualTo(LessonAiDocumentStatuses.Unsupported));
            Assert.That(unsupported.ErrorCode, Is.EqualTo(LessonAiDocumentKinds.UnsupportedErrorCode));
            Assert.That(_store.DocumentRequesters.Values, Is.All.EqualTo(Admin));
        });
    }

    [Test]
    public async Task SyncIsIdempotentAndDoesNotResetFinishedDocuments()
    {
        var attachment = _media.Add(LessonId, "bai.txt", "text/plain");
        await Sync.HandleAsync(new SyncLessonAiDocumentsCommand(CourseId, LessonId, Admin, false), CancellationToken.None);
        await _store.CompleteDocumentAsync(_store.Documents.Single().Id, "text", CancellationToken.None);

        var documents = await Sync.HandleAsync(new SyncLessonAiDocumentsCommand(CourseId, LessonId, Admin, false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_store.Documents, Has.Count.EqualTo(1));
            Assert.That(documents.Single().MediaId, Is.EqualTo(attachment.MediaId));
            Assert.That(documents.Single().Status, Is.EqualTo(LessonAiDocumentStatuses.Extracted));
        });
    }

    [Test]
    public async Task SyncRetriesFailedDocumentsOnlyWhenAsked()
    {
        _media.Add(LessonId, "bai.txt", "text/plain");
        await Sync.HandleAsync(new SyncLessonAiDocumentsCommand(CourseId, LessonId, Admin, false), CancellationToken.None);
        await _store.FailDocumentAsync(_store.Documents.Single().Id, "X", "failed", CancellationToken.None);

        var withoutRetry = await Sync.HandleAsync(new SyncLessonAiDocumentsCommand(CourseId, LessonId, Admin, false), CancellationToken.None);
        var withRetry = await Sync.HandleAsync(new SyncLessonAiDocumentsCommand(CourseId, LessonId, Admin, true), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(withoutRetry.Single().Status, Is.EqualTo(LessonAiDocumentStatuses.Failed));
            Assert.That(withRetry.Single().Status, Is.EqualTo(LessonAiDocumentStatuses.Pending));
        });
    }

    [Test]
    public void SyncRejectsAnUnknownLesson()
    {
        var exception = Assert.ThrowsAsync<CourseApplicationException>(() =>
            Sync.HandleAsync(new SyncLessonAiDocumentsCommand(CourseId, Guid.NewGuid(), Admin, false), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonNotFound));
    }

    [Test]
    public async Task ListShowsAttachmentsNotYetQueuedAsNotQueued()
    {
        _media.Add(LessonId, "bai.pdf", "application/pdf");

        var documents = await new GetLessonAiDocumentsHandler(_lessons, _media, _store).HandleAsync(CourseId, LessonId, CancellationToken.None);

        Assert.That(documents.Single().Status, Is.EqualTo(LessonAiDocumentStatuses.NotQueued));
    }

    // ---- Create job ----

    private CreateLessonAiJobHandler CreateJob => new(_lessons, _store);

    [Test]
    public async Task CreateJobStoresTheSelectionAndQuestionCount()
    {
        var mediaId = ExtractedDocument();

        var job = await CreateJob.HandleAsync(JobCommand("generate_quiz", [mediaId, mediaId], 7), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(job.JobType, Is.EqualTo(LessonAiJobTypes.GenerateQuiz));
            Assert.That(job.Status, Is.EqualTo(LessonAiJobStatuses.Pending));
            Assert.That(job.MediaIds, Is.EqualTo(new[] { mediaId }));
            Assert.That(job.QuestionCount, Is.EqualTo(7));
            Assert.That(job.CreatedBy, Is.EqualTo(Admin));
        });
    }

    [Test]
    public async Task CreateQuizJobDefaultsToTenQuestionsAndLessonJobHasNone()
    {
        var mediaId = ExtractedDocument();

        var quiz = await CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.GenerateQuiz, [mediaId]), CancellationToken.None);
        var lesson = await CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.SynthesizeLesson, [mediaId]), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.QuestionCount, Is.EqualTo(CreateLessonAiJobHandler.DefaultQuestionCount));
            Assert.That(lesson.QuestionCount, Is.Null);
        });
    }

    [TestCase(null, 1, null)]
    [TestCase("DELETE_EVERYTHING", 1, null)]
    [TestCase("GENERATE_QUIZ", 0, null)]
    [TestCase("GENERATE_QUIZ", 1, 0)]
    [TestCase("GENERATE_QUIZ", 1, 31)]
    public void CreateJobValidatesTypeSelectionAndQuestionCount(string? type, int documentCount, int? questions)
    {
        var ids = Enumerable.Range(0, documentCount).Select(_ => ExtractedDocument()).ToArray();

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => CreateJob.HandleAsync(JobCommand(type, ids, questions), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(ApiErrorCodes.ValidationFailed));
    }

    [Test]
    public void CreateJobRejectsTooManyDocuments()
    {
        var ids = Enumerable.Range(0, CreateLessonAiJobHandler.MaxDocuments + 1).Select(_ => Guid.NewGuid()).ToArray();

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.GenerateQuiz, ids), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(ApiErrorCodes.ValidationFailed));
    }

    [Test]
    public void CreateJobRejectsDocumentsThatAreNotExtractedOrDoNotBelongToTheLesson()
    {
        var pending = _store.AddExtracted(LessonId, "a.txt", "x");
        _store.Documents[_store.Documents.IndexOf(pending)] = pending with { Status = LessonAiDocumentStatuses.Pending };

        var notExtracted = Assert.ThrowsAsync<CourseApplicationException>(() => CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.GenerateQuiz, [pending.MediaId]), CancellationToken.None));
        var foreign = Assert.ThrowsAsync<CourseApplicationException>(() => CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.GenerateQuiz, [Guid.NewGuid()]), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(notExtracted!.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonAiDocumentNotReady));
            Assert.That(foreign!.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonAiDocumentNotReady));
            Assert.That(_store.Jobs, Is.Empty);
        });
    }

    [Test]
    public void CreateJobRequiresConfirmationBeforeOverwritingExistingLessonContent()
    {
        _lessons = new MutableLessonRepository(CourseId, LessonId, "Nội dung có sẵn");
        var mediaId = ExtractedDocument();

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.SynthesizeLesson, [mediaId]), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonContentExists));
    }

    [Test]
    public async Task CreateJobAllowsOverwriteWhenConfirmedAndQuizJobsIgnoreExistingContent()
    {
        _lessons = new MutableLessonRepository(CourseId, LessonId, "Nội dung có sẵn");
        var mediaId = ExtractedDocument();

        var overwrite = await CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.SynthesizeLesson, [mediaId], overwrite: true), CancellationToken.None);
        var quiz = await CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.GenerateQuiz, [mediaId]), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(overwrite.OverwriteContent, Is.True);
            Assert.That(quiz.Status, Is.EqualTo(LessonAiJobStatuses.Pending));
        });
    }

    [Test]
    public async Task CreateJobRejectsASecondActiveJobOfTheSameTypeButAllowsAnotherType()
    {
        var mediaId = ExtractedDocument();
        await CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.GenerateQuiz, [mediaId]), CancellationToken.None);

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.GenerateQuiz, [mediaId]), CancellationToken.None));
        var other = await CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.SynthesizeLesson, [mediaId]), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonAiJobActive));
            Assert.That(other.JobType, Is.EqualTo(LessonAiJobTypes.SynthesizeLesson));
        });
    }

    [Test]
    public void CreateJobRejectsAnUnknownLesson()
    {
        var mediaId = ExtractedDocument();

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() =>
            CreateJob.HandleAsync(new CreateLessonAiJobCommand(CourseId, Guid.NewGuid(), Admin, LessonAiJobTypes.GenerateQuiz, [mediaId], null, false), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonNotFound));
    }

    // ---- Read jobs ----

    [Test]
    public async Task GetJobsReturnsRecentJobsAndDetailIncludesThePreviousContent()
    {
        var mediaId = ExtractedDocument();
        var job = await CreateJob.HandleAsync(JobCommand(LessonAiJobTypes.SynthesizeLesson, [mediaId]), CancellationToken.None);
        await _store.SetJobPreviousContentAsync(job.Id, "bản cũ", CancellationToken.None);
        var handler = new GetLessonAiJobsHandler(_lessons, _store);

        var list = await handler.HandleListAsync(CourseId, LessonId, CancellationToken.None);
        var detail = await handler.HandleDetailAsync(CourseId, LessonId, job.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(list.Single().Id, Is.EqualTo(job.Id));
            Assert.That(detail.PreviousContentMarkdown, Is.EqualTo("bản cũ"));
        });
    }

    [Test]
    public void GetJobDetailReturnsNotFoundForAnotherLessonsJob()
    {
        var handler = new GetLessonAiJobsHandler(_lessons, _store);

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => handler.HandleDetailAsync(CourseId, LessonId, Guid.NewGuid(), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonAiJobNotFound));
    }
}
