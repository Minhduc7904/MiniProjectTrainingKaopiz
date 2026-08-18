// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/BatchNotificationTests.cs
// Mục đích: Kiểm thử toàn bộ use case Notification Batch: tạo batch, snapshot recipient, dispatch và truy vấn trạng thái hoặc item lỗi.

#pragma warning disable CA1707

using BuildingBlocks.Messaging.Abstractions;
using MediaService.Contracts.Messaging;
using System.Runtime.CompilerServices;
using NotificationService.Application;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Features.Batches.Create;
using NotificationService.Application.Features.Batches.Dispatch;
using NotificationService.Application.Features.Batches.Snapshot;
using NotificationService.Application.Features.Batches;
using NotificationService.Application.Content;
using NotificationService.Infrastructure.Sending;

namespace NotificationService.UnitTests;

public sealed class CreateNotificationBatchHandlerTests
{
    [Test]
    public void HandleAsync_RejectsScopeOtherThanAllStudents()
    {
        var handler = new CreateNotificationBatchHandler(
            new StubBatchRepository(),
            new StubCommandSender(),
            new NotificationMediaReferenceExtractor(),
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
    }

    [Test]
    public async Task HandleAsync_CreatesPendingBatchAndQueuesSnapshotCommand()
    {
        var repository = new StubBatchRepository();
        var commandSender = new StubCommandSender();
        var handler = new CreateNotificationBatchHandler(
            repository,
            commandSender,
            new NotificationMediaReferenceExtractor(),
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
            var createdRecord = repository.CreatedRecord;
            Assert.That(createdRecord, Is.Not.Null);
            Assert.That(createdRecord!.BatchSize, Is.EqualTo(500));
            Assert.That(commandSender.Commands, Has.Count.EqualTo(1));
            Assert.That(commandSender.Commands[0], Is.TypeOf<SnapshotNotificationBatchV1>());
            Assert.That(((SnapshotNotificationBatchV1)commandSender.Commands[0]).BatchId, Is.EqualTo(result.Id));
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
    public async Task HandleAsyncSuccessfulChunkQueuesOneMediaUsageBatchCommand()
    {
        var batchId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var createdBy = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var mediaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var firstItem = new NotificationBatchWorkItem(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            batchId,
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            0,
            "Title",
            $"![media](/media/api/media/{mediaId:D}/content)",
            createdBy);
        var secondItem = firstItem with
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            StudentId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
        };
        var repository = new StubBatchRepository([firstItem, secondItem]);
        var commandSender = new StubCommandSender();
        var handler = new DispatchNotificationBatchHandler(
            repository,
            new SuccessfulSender(),
            commandSender,
            new NotificationMediaReferenceExtractor(),
            new NotificationBatchProcessingOptions());

        await handler.HandleAsync(
            new DispatchNotificationBatchV1(batchId),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.SuccessItems, Has.Count.EqualTo(2));
            Assert.That(commandSender.Commands, Has.One.TypeOf<RegisterNotificationMediaUsageBatchV1>());
            var command = (RegisterNotificationMediaUsageBatchV1)commandSender.Commands[0];
            Assert.That(command.NotificationIds, Has.Count.EqualTo(2));
            Assert.That(command.References, Has.One.EqualTo(
                new NotificationMediaUsageReferenceV1(
                    mediaId,
                    NotificationMediaUsageTypes.Embed,
                    0)));
        });
    }

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
            commandSender,
            new NotificationMediaReferenceExtractor(),
            new NotificationBatchProcessingOptions());

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

public sealed class SnapshotNotificationBatchHandlerTests
{
    [Test]
    public async Task HandleAsync_ConcurrentDispatchConfigured_QueuesOneCommandPerSlot()
    {
        var batchId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        var repository = new StubBatchRepository
        {
            SnapshotWork = new NotificationSnapshotWork(false, true),
        };
        var commandSender = new StubCommandSender();
        var handler = new SnapshotNotificationBatchHandler(
            new StubStudentRecipientClient(),
            repository,
            commandSender,
            new NotificationBatchProcessingOptions
            {
                DispatchChunkConcurrency = 3,
            });

        await handler.HandleAsync(new SnapshotNotificationBatchV1(batchId), CancellationToken.None);

        Assert.That(
            commandSender.Commands.OfType<DispatchNotificationBatchV1>().Select(command => command.BatchId),
            Is.EqualTo(new[] { batchId, batchId, batchId }));
    }

    [Test]
    public async Task HandleAsync_RecipientsStreamed_PersistsPagesAndQueuesDispatch()
    {
        var batchId = Guid.NewGuid();
        var repository = new StubBatchRepository
        {
            SnapshotWork = new NotificationSnapshotWork(true, false),
            SnapshotCompleted = true,
        };
        var commandSender = new StubCommandSender();
        var handler = new SnapshotNotificationBatchHandler(
            new StubStudentRecipientClient(
                [Guid.Parse("11111111-1111-1111-1111-111111111111")],
                [Guid.Parse("22222222-2222-2222-2222-222222222222")]),
            repository,
            commandSender,
            new NotificationBatchProcessingOptions());

        await handler.HandleAsync(
            new SnapshotNotificationBatchV1(batchId),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.SnapshotPages, Has.Count.EqualTo(2));
            Assert.That(repository.SnapshotPages.SelectMany(page => page).ToArray().Length, Is.EqualTo(2));
            Assert.That(commandSender.Commands, Has.One.TypeOf<DispatchNotificationBatchV1>());
        });
    }

    [Test]
    public async Task HandleAsync_StudentServiceUnavailable_MarksBatchFailed()
    {
        var repository = new StubBatchRepository
        {
            SnapshotWork = new NotificationSnapshotWork(true, false),
        };
        var handler = new SnapshotNotificationBatchHandler(
            new FailingStudentRecipientClient(),
            repository,
            new StubCommandSender(),
            new NotificationBatchProcessingOptions());

        await handler.HandleAsync(new SnapshotNotificationBatchV1(Guid.NewGuid()), CancellationToken.None);

        Assert.That(repository.SnapshotMarkedFailed, Is.True);
    }
}

internal sealed class StubStudentRecipientClient(params IReadOnlyList<Guid>[] pages) : IStudentRecipientClient
{
    public bool WasCalled { get; private set; }

    public async IAsyncEnumerable<IReadOnlyList<Guid>> GetActiveStudentIdPagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        WasCalled = true;
        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return page;
        }
    }
}

internal sealed class FailingStudentRecipientClient : IStudentRecipientClient
{
    public async IAsyncEnumerable<IReadOnlyList<Guid>> GetActiveStudentIdPagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        if (cancellationToken.IsCancellationRequested)
        {
            yield break;
        }

        throw NotificationErrors.StudentServiceUnavailable();
    }
}

internal sealed class StubBatchRepository : INotificationBatchRepository
{
    public StubBatchRepository(IReadOnlyList<NotificationBatchWorkItem>? items = null, bool hasRemaining = false)
    {
        Items = items;
        HasRemaining = hasRemaining;
    }

    private IReadOnlyList<NotificationBatchWorkItem>? Items { get; }
    private bool HasRemaining { get; }
    public CreateNotificationBatchRecord? CreatedRecord { get; private set; }
    public List<Guid> FailedItems { get; } = [];
    public List<Guid> SuccessItems { get; } = [];
    public List<IReadOnlyList<Guid>> SnapshotPages { get; } = [];
    public NotificationSnapshotWork SnapshotWork { get; set; } = new(false, false);
    public bool SnapshotCompleted { get; set; }
    public bool SnapshotMarkedFailed { get; private set; }

    public Task<NotificationBatchSummary> CreateAsync(
        CreateNotificationBatchRecord record,
        CancellationToken cancellationToken)
    {
        CreatedRecord = record;
        return Task.FromResult(new NotificationBatchSummary(
            record.Id,
            "PENDING",
            0,
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

    public Task<NotificationBatchFailedItemsPage> GetFailedItemsAsync(
        Guid batchId,
        Guid? afterItemId,
        int limit,
        CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationBatchFailedItemsPage([], null, false));

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<NotificationSnapshotWork> PrepareSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(SnapshotWork);

    public Task AppendSnapshotPageAsync(Guid batchId, IReadOnlyList<Guid> studentIds, CancellationToken cancellationToken)
    {
        SnapshotPages.Add(studentIds);
        return Task.CompletedTask;
    }

    public Task<bool> CompleteSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(SnapshotCompleted);

    public Task MarkSnapshotFailedAsync(Guid batchId, CancellationToken cancellationToken)
    {
        SnapshotMarkedFailed = true;
        return Task.CompletedTask;
    }

    public Task<NotificationBatchClaim?> ClaimChunkAsync(
        Guid batchId,
        CancellationToken cancellationToken) =>
        Task.FromResult<NotificationBatchClaim?>(
            Items is null
                ? null
                : new NotificationBatchClaim(batchId, Guid.NewGuid(), Items));

    public Task<IReadOnlyList<NotificationSummary>> CompleteClaimAsync(
        NotificationBatchClaim claim,
        IReadOnlyList<NotificationBatchDeliveryResult> results,
        CancellationToken cancellationToken)
    {
        var itemsById = claim.Items.ToDictionary(item => item.Id);
        var notifications = new List<NotificationSummary>();
        foreach (var result in results)
        {
            var item = itemsById[result.ItemId];
            if (!result.IsSuccess)
            {
                FailedItems.Add(item.Id);
                continue;
            }

            SuccessItems.Add(item.Id);
            notifications.Add(new NotificationSummary(
                Guid.NewGuid(),
                item.StudentId,
                item.Title,
                item.BodyMarkdown,
                "BULK",
                "UNREAD",
                item.CreatedBy,
                DateTime.UnixEpoch,
                null));
        }

        return Task.FromResult<IReadOnlyList<NotificationSummary>>(notifications);
    }

    public Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(HasRemaining);
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

internal sealed class SuccessfulSender : INotificationSender
{
    public Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
