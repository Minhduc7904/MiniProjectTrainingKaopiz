using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.Quizzes;
using CourseService.Application.UseCases.Quizzes.Create;
using CourseService.Application.UseCases.Quizzes.Delete;
using CourseService.Application.UseCases.Quizzes.Update;
using CourseService.Domain.Constants;
using CourseService.UnitTests.Support;
using MediaService.Contracts.Messaging;

namespace CourseService.UnitTests.Application.Quizzes;

public sealed class QuizAdminHandlersTests
{
    private static readonly Guid ActorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid MediaA = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid MediaB = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static string Image(Guid id) => $"![ảnh](/media/api/media/{id}/content)";

    [Test]
    public async Task Create_ValidInput_PersistsTreeAndSkipsMediaSyncWithoutImages()
    {
        var store = new InMemoryQuizStore();
        var sender = new RecordingCommandSender();

        var quiz = await new CreateQuizHandler(store, sender).HandleAsync(
            new CreateQuizCommand(store.CourseId, store.LessonId, ActorId, QuizBuilders.ValidInput()), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Status, Is.EqualTo(QuizStatuses.Draft));
            Assert.That(quiz.Questions, Has.Count.EqualTo(2));
            Assert.That(quiz.TotalPoints, Is.EqualTo(5m));
            Assert.That(sender.Commands, Is.Empty);
        });
    }

    [Test]
    public async Task Create_ImageInQuestionAndOption_SendsQuizContentUsageSync()
    {
        var store = new InMemoryQuizStore();
        var sender = new RecordingCommandSender();
        var input = new QuizInput("Quiz", Image(MediaA), null,
        [
            new("Câu hỏi", QuizQuestionTypes.SingleChoice, 1m, [new(Image(MediaB), true), new("B", false)]),
        ]);

        var quiz = await new CreateQuizHandler(store, sender).HandleAsync(
            new CreateQuizCommand(store.CourseId, store.LessonId, ActorId, input), CancellationToken.None);

        var command = sender.Commands.Single() as SynchronizeMarkdownMediaUsageV1;
        Assert.Multiple(() =>
        {
            Assert.That(command?.OwnerType, Is.EqualTo(MarkdownMediaUsageOwnerTypes.QuizContent));
            Assert.That(command?.OwnerId, Is.EqualTo(quiz.Id));
            Assert.That(command?.Added.Select(reference => reference.MediaId), Is.EquivalentTo([MediaA, MediaB]));
            Assert.That(command?.Removed, Is.Empty);
        });
    }

    [Test]
    public void Create_UnknownLesson_ThrowsLessonNotFound()
    {
        var store = new InMemoryQuizStore();

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => new CreateQuizHandler(store, new RecordingCommandSender())
            .HandleAsync(new CreateQuizCommand(store.CourseId, Guid.NewGuid(), ActorId, QuizBuilders.ValidInput()), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.LessonNotFound));
    }

    [Test]
    public void Create_InvalidInput_ThrowsValidationFailedBeforeTouchingStore()
    {
        var store = new InMemoryQuizStore();

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => new CreateQuizHandler(store, new RecordingCommandSender())
            .HandleAsync(new CreateQuizCommand(store.CourseId, store.LessonId, ActorId, new QuizInput("", null, null, [])), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(store.Quizzes, Is.Empty);
        });
    }

    [Test]
    public async Task Update_RemovingAndAddingImages_SendsDiff()
    {
        var store = new InMemoryQuizStore();
        var sender = new RecordingCommandSender();
        var created = await new CreateQuizHandler(store, sender).HandleAsync(
            new CreateQuizCommand(store.CourseId, store.LessonId, ActorId,
                new QuizInput("Quiz", Image(MediaA), null, [new("Q", QuizQuestionTypes.SingleChoice, 1m, [new("A", true), new("B", false)])])),
            CancellationToken.None);
        sender.Commands.Clear();

        await new UpdateQuizHandler(store, sender).HandleAsync(
            new UpdateQuizCommand(store.CourseId, store.LessonId, created.Id, ActorId,
                new QuizInput("Quiz", Image(MediaB), QuizStatuses.Draft, null)),
            CancellationToken.None);

        var command = (SynchronizeMarkdownMediaUsageV1)sender.Commands.Single();
        Assert.Multiple(() =>
        {
            Assert.That(command.Added.Select(reference => reference.MediaId), Is.EquivalentTo([MediaB]));
            Assert.That(command.Removed.Select(reference => reference.MediaId), Is.EquivalentTo([MediaA]));
        });
    }

    [Test]
    public async Task Update_WithoutQuestions_KeepsExistingTree()
    {
        var store = new InMemoryQuizStore();
        var sender = new RecordingCommandSender();
        var created = await new CreateQuizHandler(store, sender).HandleAsync(
            new CreateQuizCommand(store.CourseId, store.LessonId, ActorId, QuizBuilders.ValidInput()), CancellationToken.None);

        var updated = await new UpdateQuizHandler(store, sender).HandleAsync(
            new UpdateQuizCommand(store.CourseId, store.LessonId, created.Id, ActorId, new QuizInput("Tên mới", null, QuizStatuses.Published, null)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(updated.Title, Is.EqualTo("Tên mới"));
            Assert.That(updated.Status, Is.EqualTo(QuizStatuses.Published));
            Assert.That(updated.Questions, Is.EqualTo(created.Questions));
        });
    }

    [Test]
    public async Task Update_ReplacingQuestionsWhenAttemptsExist_ThrowsQuizHasAttempts()
    {
        var store = new InMemoryQuizStore();
        var sender = new RecordingCommandSender();
        var created = await new CreateQuizHandler(store, sender).HandleAsync(
            new CreateQuizCommand(store.CourseId, store.LessonId, ActorId, QuizBuilders.ValidInput(QuizStatuses.Published)), CancellationToken.None);
        await store.StartAsync(created.Id, Guid.NewGuid(), CancellationToken.None);

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => new UpdateQuizHandler(store, sender).HandleAsync(
            new UpdateQuizCommand(store.CourseId, store.LessonId, created.Id, ActorId, QuizBuilders.ValidInput()), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.QuizHasAttempts));
    }

    [Test]
    public async Task Update_MetadataOnlyWhenAttemptsExist_IsAllowed()
    {
        var store = new InMemoryQuizStore();
        var sender = new RecordingCommandSender();
        var created = await new CreateQuizHandler(store, sender).HandleAsync(
            new CreateQuizCommand(store.CourseId, store.LessonId, ActorId, QuizBuilders.ValidInput(QuizStatuses.Published)), CancellationToken.None);
        await store.StartAsync(created.Id, Guid.NewGuid(), CancellationToken.None);

        var updated = await new UpdateQuizHandler(store, sender).HandleAsync(
            new UpdateQuizCommand(store.CourseId, store.LessonId, created.Id, ActorId, new QuizInput("Đổi tên", null, QuizStatuses.Draft, null)),
            CancellationToken.None);

        Assert.That(updated.Title, Is.EqualTo("Đổi tên"));
    }

    [Test]
    public void Update_UnknownQuiz_ThrowsQuizNotFound()
    {
        var store = new InMemoryQuizStore();

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => new UpdateQuizHandler(store, new RecordingCommandSender())
            .HandleAsync(new UpdateQuizCommand(store.CourseId, store.LessonId, Guid.NewGuid(), ActorId, QuizBuilders.ValidInput()), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.QuizNotFound));
    }

    [Test]
    public async Task Delete_ExistingQuiz_RemovesItAndQueuesUsageCleanup()
    {
        var store = new InMemoryQuizStore();
        var sender = new RecordingCommandSender();
        var created = await new CreateQuizHandler(store, sender).HandleAsync(
            new CreateQuizCommand(store.CourseId, store.LessonId, ActorId, QuizBuilders.ValidInput()), CancellationToken.None);
        var usageId = Guid.NewGuid();
        var reader = new StubMediaReader([usageId]);

        await new DeleteQuizHandler(store, reader, sender).HandleAsync(store.CourseId, store.LessonId, created.Id, ActorId, CancellationToken.None);

        var command = sender.Commands.OfType<DeleteMediaUsagesByIdsV1>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(store.Quizzes, Is.Empty);
            Assert.That(reader.LastOwners, Is.EquivalentTo(CourseMediaUsageScopes.ForQuiz(created.Id)));
            Assert.That(command.UsageIds, Is.EquivalentTo([usageId]));
        });
    }

    [Test]
    public async Task Delete_QuizWithoutUsages_SendsNothing()
    {
        var store = new InMemoryQuizStore();
        var sender = new RecordingCommandSender();
        var created = await new CreateQuizHandler(store, sender).HandleAsync(
            new CreateQuizCommand(store.CourseId, store.LessonId, ActorId, QuizBuilders.ValidInput()), CancellationToken.None);

        await new DeleteQuizHandler(store, new StubMediaReader([]), sender).HandleAsync(store.CourseId, store.LessonId, created.Id, ActorId, CancellationToken.None);

        Assert.That(sender.Commands, Is.Empty);
    }

    [Test]
    public void Delete_UnknownQuiz_ThrowsQuizNotFound()
    {
        var store = new InMemoryQuizStore();

        var exception = Assert.ThrowsAsync<CourseApplicationException>(() => new DeleteQuizHandler(store, new StubMediaReader([]), new RecordingCommandSender())
            .HandleAsync(store.CourseId, store.LessonId, Guid.NewGuid(), ActorId, CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(CourseErrorCodes.QuizNotFound));
    }

    private sealed class StubMediaReader(IReadOnlyList<Guid> usageIds) : ICourseMediaReader
    {
        public IReadOnlyList<CourseMediaUsageOwnerScope> LastOwners { get; private set; } = [];

        public Task<IReadOnlyList<Guid>> GetActiveUsageIdsAsync(IReadOnlyList<CourseMediaUsageOwnerScope> owners, CancellationToken cancellationToken)
        {
            LastOwners = owners;
            return Task.FromResult(usageIds);
        }

        public Task<CourseMediaSet> GetAsync(Guid courseId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyDictionary<Guid, CourseMediaSet>> GetManyAsync(IReadOnlyList<Guid> courseIds, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<CourseMediaAsset>> GetLessonAttachmentsAsync(Guid lessonId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
