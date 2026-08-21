using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.Courses.Delete;
using MediaService.Contracts.Messaging;

namespace CourseService.UnitTests.Application.Courses.Delete;

public sealed class DeleteCourseHandlerTests
{
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LessonId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ActorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CourseUsageId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid LessonUsageId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Test]
    public async Task HandleAsync_ExistingCourse_DeletesAggregateAndQueuesEveryUsageId()
    {
        var repository = new StubCourseCommandRepository(CourseId, [LessonId]);
        var mediaReader = new StubCourseMediaReader([CourseUsageId, LessonUsageId]);
        var commandSender = new StubCommandSender();
        var handler = new DeleteCourseHandler(repository, mediaReader, commandSender);

        await handler.HandleAsync(CourseId, ActorId, CancellationToken.None);

        var command = commandSender.Commands.Single() as DeleteMediaUsagesByIdsV1;
        Assert.Multiple(() =>
        {
            Assert.That(repository.DeletedCourseId, Is.EqualTo(CourseId));
            Assert.That(mediaReader.LastOwners, Is.EquivalentTo([
                new CourseMediaUsageOwnerScope("COURSE", MarkdownMediaUsageOwnerTypes.CourseDescription, CourseId),
                new CourseMediaUsageOwnerScope("COURSE", MarkdownMediaUsageOwnerTypes.CourseThumbnail, CourseId),
                new CourseMediaUsageOwnerScope("COURSE", MarkdownMediaUsageOwnerTypes.CourseGallery, CourseId),
                new CourseMediaUsageOwnerScope("COURSE", MarkdownMediaUsageOwnerTypes.LessonContent, LessonId),
                new CourseMediaUsageOwnerScope("COURSE", MarkdownMediaUsageOwnerTypes.LessonAttachment, LessonId),
            ]));
            Assert.That(command?.UsageIds, Is.EquivalentTo([CourseUsageId, LessonUsageId]));
        });
    }

    private sealed class StubCourseCommandRepository(Guid courseId, IReadOnlyList<Guid> lessonIds) : ICourseCommandRepository
    {
        public Guid? DeletedCourseId { get; private set; }

        public Task<CourseCommandRecord?> GetAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<CourseCommandRecord?>(id == courseId
                ? new CourseCommandRecord(courseId, "Backend Fundamentals", null, "DRAFT", DateTime.UtcNow, DateTime.UtcNow)
                : null);

        public Task<IReadOnlyList<Guid>> GetLessonIdsAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(id == courseId ? lessonIds : (IReadOnlyList<Guid>)[]);

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            DeletedCourseId = id;
            return Task.FromResult(id == courseId);
        }

        public Task<CourseCommandRecord> CreateAsync(string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<CourseCommandRecord?> UpdateAsync(Guid courseId, string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class StubCourseMediaReader(IReadOnlyList<Guid> usageIds) : ICourseMediaReader
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

    private sealed class StubCommandSender : ICommandSender
    {
        public List<ICommand> Commands { get; } = [];

        public Task SendAsync<TCommand>(string destinationService, TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, ICommand
        {
            Commands.Add(command);
            return Task.CompletedTask;
        }
    }
}
