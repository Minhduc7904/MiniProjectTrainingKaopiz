// File: backend/Services/Notification/NotificationService.UnitTests/Domain/Entities/NotificationBatchStateTests.cs
// Mục đích: Khóa các transition snapshot, dispatch và terminal status của Notification Batch ngoài EF repository.

using NotificationService.Domain.Constants;
using NotificationService.Domain.Entities;

namespace NotificationService.UnitTests.Domain.Entities;

[TestFixture]
public sealed class NotificationBatchStateTests
{
    [Test]
    public void PrepareSnapshotPendingBatchTransitionsToSnapshottingAndRequestsRecipients()
    {
        var batch = CreateBatch(NotificationBatchStatuses.Pending);
        var decision = batch.PrepareSnapshot();

        Assert.Multiple(() =>
        {
            Assert.That(decision, Is.EqualTo(NotificationSnapshotDecision.ReadRecipients));
            Assert.That(batch.Status, Is.EqualTo(NotificationBatchStatuses.Snapshotting));
        });
    }

    [Test]
    public void CompleteSnapshotEmptyRecipientsMarksBatchFailed()
    {
        var completedAt = DateTime.UnixEpoch.AddMinutes(1);
        var batch = CreateBatch(NotificationBatchStatuses.Snapshotting);
        var shouldDispatch = batch.CompleteSnapshot(0, completedAt);

        Assert.Multiple(() =>
        {
            Assert.That(shouldDispatch, Is.False);
            Assert.That(batch.Status, Is.EqualTo(NotificationBatchStatuses.Failed));
            Assert.That(batch.CompletedAtUtc, Is.EqualTo(completedAt));
        });
    }

    [TestCase(0u, true, NotificationBatchItemStatuses.Success, 0u)]
    [TestCase(0u, false, NotificationBatchItemStatuses.Retry, 1u)]
    [TestCase(1u, false, NotificationBatchItemStatuses.Failed, 2u)]
    public void ApplyDeliveryResultPreservesExistingRetryContract(
        uint retryCount, bool success, string expectedStatus, uint expectedRetryCount)
    {
        var item = new NotificationBatchItemState(NotificationBatchItemStatuses.Processing, retryCount);
        item.ApplyDeliveryResult(success, "safe-error", DateTime.UnixEpoch);

        Assert.Multiple(() =>
        {
            Assert.That(item.Status, Is.EqualTo(expectedStatus));
            Assert.That(item.RetryCount, Is.EqualTo(expectedRetryCount));
        });
    }

    [TestCase(2u, 0u, NotificationBatchStatuses.Completed)]
    [TestCase(0u, 2u, NotificationBatchStatuses.Failed)]
    [TestCase(1u, 1u, NotificationBatchStatuses.PartialFailed)]
    public void FinalizeBatchNoRemainingWorkSelectsTerminalStatus(
        uint successCount, uint failedCount, string expectedStatus)
    {
        var batch = CreateBatch(NotificationBatchStatuses.Processing, successCount, failedCount);
        var hasRemaining = batch.FinalizeOrHasRemaining(false, false, DateTime.UnixEpoch);

        Assert.Multiple(() =>
        {
            Assert.That(hasRemaining, Is.False);
            Assert.That(batch.Status, Is.EqualTo(expectedStatus));
        });
    }

    private static NotificationBatchState CreateBatch(
        string status, uint successCount = 0, uint failedCount = 0) =>
        new(status, 0, successCount + failedCount, successCount, failedCount, null, null);
}
