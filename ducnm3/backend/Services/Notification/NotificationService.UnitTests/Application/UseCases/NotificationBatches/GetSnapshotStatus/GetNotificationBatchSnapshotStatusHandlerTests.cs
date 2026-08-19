// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/GetSnapshotStatus/GetNotificationBatchSnapshotStatusHandlerTests.cs
// Mục đích: Kiểm thử API model của bước snapshot, gồm trạng thái chạy, số recipient đã chụp và phần trăm khi có requestedCount.

#pragma warning disable CA1707

using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.UseCases.NotificationBatches.GetSnapshotStatus;
using NotificationService.Domain.Constants;
using NotificationService.UnitTests.Application.UseCases.NotificationBatches.TestDoubles;

namespace NotificationService.UnitTests.Application.UseCases.NotificationBatches.GetSnapshotStatus;

public sealed class GetNotificationBatchSnapshotStatusHandlerTests
{
    private static readonly Guid BatchId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public async Task HandleAsync_SnapshottingWithRequestedCount_ReturnsRunningProgress()
    {
        var repository = new StubBatchRepository
        {
            SnapshotProgress = new NotificationBatchSnapshotProgress(
                BatchId,
                NotificationBatchStatuses.Snapshotting,
                3000,
                1200,
                null),
        };
        var sut = new GetNotificationBatchSnapshotStatusHandler(repository);

        var result = await sut.HandleAsync(BatchId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(NotificationBatchStepStatuses.Running));
            Assert.That(result.SnapshotCount, Is.EqualTo(1200));
            Assert.That(result.TargetCount, Is.EqualTo(3000));
            Assert.That(result.ProgressPercent, Is.EqualTo(40m));
        });
    }

    [TestCase(NotificationBatchStatuses.Pending, NotificationBatchStepStatuses.Pending)]
    [TestCase(NotificationBatchStatuses.SnapshotReady, NotificationBatchStepStatuses.Completed)]
    [TestCase(NotificationBatchStatuses.Processing, NotificationBatchStepStatuses.Completed)]
    [TestCase(NotificationBatchStatuses.Completed, NotificationBatchStepStatuses.Completed)]
    public async Task HandleAsync_BatchStatus_MapsSnapshotStepStatus(
        string batchStatus,
        string expectedStepStatus)
    {
        var repository = new StubBatchRepository
        {
            SnapshotProgress = new NotificationBatchSnapshotProgress(
                BatchId, batchStatus, null, 20, 20),
        };
        var sut = new GetNotificationBatchSnapshotStatusHandler(repository);

        var result = await sut.HandleAsync(BatchId, CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(expectedStepStatus));
    }

    [Test]
    public async Task HandleAsync_CompletedBelowRequestedCount_UsesActualSnapshotAsTarget()
    {
        var repository = new StubBatchRepository
        {
            SnapshotProgress = new NotificationBatchSnapshotProgress(
                BatchId, NotificationBatchStatuses.SnapshotReady, 3000, 20, 20),
        };
        var sut = new GetNotificationBatchSnapshotStatusHandler(repository);

        var result = await sut.HandleAsync(BatchId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TargetCount, Is.EqualTo(20));
            Assert.That(result.ProgressPercent, Is.EqualTo(100m));
        });
    }
}
