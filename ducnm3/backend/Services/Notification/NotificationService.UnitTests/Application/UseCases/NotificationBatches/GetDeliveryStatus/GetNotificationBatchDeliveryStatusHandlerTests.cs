// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/GetDeliveryStatus/GetNotificationBatchDeliveryStatusHandlerTests.cs
// Mục đích: Kiểm thử trạng thái và phần trăm của bước gửi notification được suy ra từ counter batch hiện có.

#pragma warning disable CA1707

using NotificationService.Application.Repositories.Models;
using NotificationService.Application.UseCases.NotificationBatches.GetDeliveryStatus;
using NotificationService.Domain.Constants;
using NotificationService.UnitTests.Application.UseCases.NotificationBatches.TestDoubles;

namespace NotificationService.UnitTests.Application.UseCases.NotificationBatches.GetDeliveryStatus;

public sealed class GetNotificationBatchDeliveryStatusHandlerTests
{
    private static readonly Guid BatchId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public async Task HandleAsync_Processing_ReturnsCountersAndProgress()
    {
        var repository = new StubBatchRepository
        {
            BatchSummary = new NotificationBatchSummary(
                BatchId, "Batch", NotificationBatchStatuses.Processing,
                3000, 1500, 1490, 10, 500, 3000, null,
                DateTime.UnixEpoch, DateTime.UnixEpoch.AddSeconds(2), null),
        };
        var sut = new GetNotificationBatchDeliveryStatusHandler(
            repository,
            new FixedTimeProvider(DateTimeOffset.UnixEpoch.AddSeconds(12)));

        var result = await sut.HandleAsync(BatchId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(NotificationBatchStepStatuses.Running));
            Assert.That(result.ProgressPercent, Is.EqualTo(50m));
            Assert.That(result.RemainingCount, Is.EqualTo(1500));
            Assert.That(result.SuccessCount, Is.EqualTo(1490));
            Assert.That(result.FailedCount, Is.EqualTo(10));
            Assert.That(result.DurationMs, Is.EqualTo(10000));
        });
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
