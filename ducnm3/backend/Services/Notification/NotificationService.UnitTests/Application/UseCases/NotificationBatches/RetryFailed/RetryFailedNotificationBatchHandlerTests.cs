// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/RetryFailed/RetryFailedNotificationBatchHandlerTests.cs
// Mục đích: Kiểm thử retry chỉ nhận batch terminal có FAILED và phát snapshot cho batch con thay vì mở lại batch nguồn.

#pragma warning disable CA1707

using NotificationService.Application.Common.Errors;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.UseCases.NotificationBatches.RetryFailed;
using NotificationService.UnitTests.Application.UseCases.NotificationBatches.TestDoubles;
using MediaService.Contracts.Messaging;

namespace NotificationService.UnitTests.Application.UseCases.NotificationBatches.RetryFailed;

public sealed class RetryFailedNotificationBatchHandlerTests
{
    [Test]
    public async Task HandleAsync_PartialFailed_CreatesChildAndQueuesSnapshot()
    {
        var sourceId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var repository = new StubBatchRepository
        {
            BatchSummary = Summary(sourceId, "PARTIAL_FAILED", 2),
            RetrySummary = Summary(childId, "PENDING", 0) with { SourceBatchId = sourceId },
        };
        var sender = new StubCommandSender();
        var handler = new RetryFailedNotificationBatchHandler(repository, sender, TimeProvider.System);

        var result = await handler.HandleAsync(sourceId, Guid.NewGuid(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(childId));
            Assert.That(result.SourceBatchId, Is.EqualTo(sourceId));
            Assert.That(sender.Commands.OfType<SnapshotNotificationBatchV1>().Single().BatchId,
                Is.EqualTo(childId));
            Assert.That(sender.Commands.OfType<StartNotificationMediaUsageJobV1>().Single().JobId,
                Is.EqualTo(childId));
        });
    }

    [TestCase("PROCESSING", 1u)]
    [TestCase("COMPLETED", 0u)]
    public void HandleAsync_NotEligible_IsConflict(string status, uint failedCount)
    {
        var sourceId = Guid.NewGuid();
        var repository = new StubBatchRepository
        {
            BatchSummary = Summary(sourceId, status, failedCount),
        };
        var handler = new RetryFailedNotificationBatchHandler(
            repository, new StubCommandSender(), TimeProvider.System);

        var exception = Assert.ThrowsAsync<NotificationApplicationException>(() =>
            handler.HandleAsync(sourceId, Guid.NewGuid(), CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task HandleAsync_RetryAlreadyExists_ReturnsChildWithoutDuplicateSnapshotCommand()
    {
        var sourceId = Guid.NewGuid();
        var child = Summary(Guid.NewGuid(), "PENDING", 0) with { SourceBatchId = sourceId };
        var repository = new StubBatchRepository
        {
            BatchSummary = Summary(sourceId, "PARTIAL_FAILED", 1),
            RetrySummary = child,
            RetryIsNew = false,
        };
        var sender = new StubCommandSender();
        var handler = new RetryFailedNotificationBatchHandler(repository, sender, TimeProvider.System);

        var result = await handler.HandleAsync(sourceId, Guid.NewGuid(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(child.Id));
            Assert.That(sender.Commands, Is.Empty);
        });
    }

    private static NotificationBatchSummary Summary(Guid id, string status, uint failedCount) =>
        new(id, "Batch", status, 10, 10, 10 - failedCount, failedCount, 500,
            10, null, DateTime.UnixEpoch, DateTime.UnixEpoch, DateTime.UnixEpoch);
}
