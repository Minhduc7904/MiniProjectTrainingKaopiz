// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/Snapshot/SnapshotNotificationBatchHandlerTests.cs
// Mục đích: Kiểm thử snapshot recipient theo page, số slot dispatch và xử lý Student Service không khả dụng.

#pragma warning disable CA1707

using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.UseCases.NotificationBatches;
using NotificationService.Application.UseCases.NotificationBatches.Dispatch;
using NotificationService.Application.UseCases.NotificationBatches.Snapshot;
using NotificationService.UnitTests.Application.UseCases.NotificationBatches.TestDoubles;

namespace NotificationService.UnitTests.Application.UseCases.NotificationBatches.Snapshot;

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
            new NotificationBatchProcessingOptions { DispatchChunkConcurrency = 3 });

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

        await handler.HandleAsync(new SnapshotNotificationBatchV1(batchId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.SnapshotPages, Has.Count.EqualTo(2));
            Assert.That(repository.SnapshotPages.SelectMany(page => page).ToArray(), Has.Length.EqualTo(2));
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
