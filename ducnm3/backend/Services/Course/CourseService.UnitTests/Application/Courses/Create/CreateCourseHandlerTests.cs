using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.Create;
using CourseService.Domain.Constants;
using MediaService.Contracts.Messaging;

namespace CourseService.UnitTests.Application.Courses.Create;

public sealed class CreateCourseHandlerTests
{
    private static readonly Guid ActorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MediaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Test]
    public async Task HandleAsyncCreatesDraftAndSynchronizesMediaEmbeddedInDescription()
    {
        var repository = new StubCourseCommandRepository(CourseId);
        var commandSender = new StubCommandSender();
        var handler = new CreateCourseHandler(repository, commandSender);
        var descriptionMarkdown = $"![cover]({ApiRoutes.Media.ContentPublicPath(MediaId)})";

        var result = await handler.HandleAsync(
            "  Backend Fundamentals  ",
            descriptionMarkdown,
            ActorId,
            CancellationToken.None);
        var synchronization = commandSender.Commands.Single() as SynchronizeCourseContentMediaUsageV1;

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(CourseStatuses.Draft));
            Assert.That(repository.LastName, Is.EqualTo("Backend Fundamentals"));
            Assert.That(repository.LastStatus, Is.EqualTo(CourseStatuses.Draft));
            Assert.That(commandSender.Commands, Has.Count.EqualTo(1));
            Assert.That(synchronization, Is.Not.Null);
            Assert.That(synchronization?.OwnerId, Is.EqualTo(CourseId));
            Assert.That(synchronization?.OwnerType, Is.EqualTo(CourseContentMediaOwnerTypes.CourseDescription));
            Assert.That(synchronization?.CreatedBy, Is.EqualTo(ActorId));
            Assert.That(synchronization?.Added, Is.EqualTo([
                new NotificationMediaUsageReferenceV1(MediaId, NotificationMediaUsageTypes.Embed, 0),
            ]));
            Assert.That(synchronization?.Removed, Is.Empty);
        });
    }

    private sealed class StubCourseCommandRepository(Guid courseId) : ICourseCommandRepository
    {
        public string? LastName { get; private set; }
        public string? LastStatus { get; private set; }

        public Task<CourseCommandRecord?> GetAsync(Guid courseId, CancellationToken cancellationToken) =>
            Task.FromResult<CourseCommandRecord?>(null);

        public Task<CourseCommandRecord> CreateAsync(string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken)
        {
            LastName = name;
            LastStatus = status;
            var now = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc);
            return Task.FromResult(new CourseCommandRecord(courseId, name, descriptionMarkdown, status, now, now));
        }

        public Task<CourseCommandRecord?> UpdateAsync(Guid courseId, string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken) =>
            Task.FromResult<CourseCommandRecord?>(null);
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
