using BuildingBlocks.Messaging.Abstractions;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Features.Batches.Create;
using NotificationService.Application.Features.Batches.Dispatch;
using NotificationService.Infrastructure.Sending;

namespace NotificationService.UnitTests;

public sealed class CreateNotificationBatchHandlerTests
{
    [Test]
    public void HandleAsync_RejectsScopeOtherThanAllStudents()
    {
        var studentClient = new StubStudentRecipientClient([Guid.NewGuid()]);
        var handler = new CreateNotificationBatchHandler(
            studentClient,
            new StubBatchRepository(),
            new StubCommandSender(),
            TimeProvider.System);

        var exception = Assert.ThrowsAsync<NotificationService.Application.NotificationApplicationException>(
            () => handler.HandleAsync(
                new CreateNotificationBatchCommand(
                    "Title",
                    "Body",
                    "COURSE_ENROLLED",
                    Guid.NewGuid(),
                    null,
                    null),
                CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(400));
        Assert.That(studentClient.WasCalled, Is.False);
    }

    [Test]
    public async Task HandleAsync_CreatesSnapshotAndDispatchesCommand()
    {
        var repository = new StubBatchRepository();
        var commandSender = new StubCommandSender();
        var handler = new CreateNotificationBatchHandler(
            new StubStudentRecipientClient([Guid.NewGuid(), Guid.NewGuid()]),
            repository,
            commandSender,
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateNotificationBatchCommand(
                "Title",
                "Body",
                "ALL_STUDENTS",
                Guid.NewGuid(),
                null,
                null),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.CreatedRecord, Is.Not.Null);
            Assert.That(repository.CreatedRecord!.StudentIds, Has.Count.EqualTo(2));
            Assert.That(repository.CreatedRecord.BatchSize, Is.EqualTo(500));
            Assert.That(commandSender.Commands, Has.Count.EqualTo(1));
            Assert.That(commandSender.Commands[0], Is.TypeOf<DispatchNotificationBatchV1>());
            Assert.That(((DispatchNotificationBatchV1)commandSender.Commands[0]).BatchId, Is.EqualTo(result.Id));
        });
    }
}

public sealed class FakeNotificationSenderTests
{
    [Test]
    public void SendAsync_FailsFirstAttemptForStudentHashDivisibleByTwenty()
    {
        var sender = new FakeNotificationSender();
        var studentId = FindStudentId(value => value % 20 == 0);

        Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.SendAsync(studentId, 1, CancellationToken.None));
    }

    [Test]
    public void SendAsync_FailsSecondAttemptForStudentHashDivisibleByOneHundred()
    {
        var sender = new FakeNotificationSender();
        var studentId = FindStudentId(value => value % 100 == 0);

        Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.SendAsync(studentId, 2, CancellationToken.None));
    }

    [Test]
    public async Task SendAsync_SucceedsWhenFailureRuleDoesNotMatch()
    {
        var sender = new FakeNotificationSender();
        var studentId = FindStudentId(value => value % 20 != 0);

        await sender.SendAsync(studentId, 1, CancellationToken.None);
    }

    private static Guid FindStudentId(Func<uint, bool> predicate)
    {
        for (var value = 1; value < 100_000; value++)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            var studentId = new Guid(bytes);
            if (predicate(unchecked((uint)studentId.GetHashCode())))
            {
                return studentId;
            }
        }

        throw new InvalidOperationException("Could not find a deterministic fake-sender test value.");
    }
}

public sealed class DispatchNotificationBatchHandlerTests
{
    [Test]
    public async Task HandleAsync_FirstBusinessFailureMarksItemForRetryAndRequeues()
    {
        var item = new NotificationBatchWorkItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            0,
            "Title",
            "Body",
            Guid.NewGuid());
        var repository = new StubBatchRepository([item], hasRemaining: true);
        var commandSender = new StubCommandSender();
        var handler = new DispatchNotificationBatchHandler(
            repository,
            new ThrowingSender(),
            commandSender);

        await handler.HandleAsync(
            new DispatchNotificationBatchV1(item.BatchId),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.FailedItems, Has.One.EqualTo(item.Id));
            Assert.That(repository.SuccessItems, Is.Empty);
            Assert.That(commandSender.Commands, Has.Count.EqualTo(1));
            Assert.That(commandSender.Commands[0], Is.TypeOf<DispatchNotificationBatchV1>());
        });
    }
}

internal sealed class StubStudentRecipientClient(IReadOnlyList<Guid> studentIds) : IStudentRecipientClient
{
    public bool WasCalled { get; private set; }

    public Task<IReadOnlyList<Guid>> GetAllActiveStudentIdsAsync(CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.FromResult(studentIds);
    }
}

internal sealed class StubBatchRepository(
    IReadOnlyList<NotificationBatchWorkItem>? items = null,
    bool hasRemaining = false) : INotificationBatchRepository
{
    public CreateNotificationBatchRecord? CreatedRecord { get; private set; }
    public List<Guid> FailedItems { get; } = [];
    public List<Guid> SuccessItems { get; } = [];

    public Task<NotificationBatchSummary> CreateAsync(
        CreateNotificationBatchRecord record,
        CancellationToken cancellationToken)
    {
        CreatedRecord = record;
        return Task.FromResult(new NotificationBatchSummary(
            record.Id,
            "PENDING",
            checked((uint)record.StudentIds.Count),
            0,
            0,
            0,
            record.BatchSize,
            record.CreatedAtUtc,
            null,
            null));
    }

    public Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult<NotificationBatchSummary?>(null);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<IReadOnlyList<NotificationBatchWorkItem>> ClaimChunkAsync(
        Guid batchId,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<NotificationBatchWorkItem>>(items ?? []);

    public Task MarkSuccessAsync(NotificationBatchWorkItem item, CancellationToken cancellationToken)
    {
        SuccessItems.Add(item.Id);
        return Task.CompletedTask;
    }

    public Task MarkFailureAsync(
        NotificationBatchWorkItem item,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        FailedItems.Add(item.Id);
        return Task.CompletedTask;
    }

    public Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(hasRemaining);
}

internal sealed class StubCommandSender : ICommandSender
{
    public List<ICommand> Commands { get; } = [];

    public Task SendAsync<TCommand>(
        string destinationService,
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
    {
        Commands.Add(command);
        return Task.CompletedTask;
    }
}

internal sealed class ThrowingSender : INotificationSender
{
    public Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken) =>
        Task.FromException(new InvalidOperationException("Expected sender failure."));
}
