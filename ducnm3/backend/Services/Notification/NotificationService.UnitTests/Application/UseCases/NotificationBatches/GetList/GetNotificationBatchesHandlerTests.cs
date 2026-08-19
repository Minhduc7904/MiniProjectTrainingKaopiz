// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/GetList/GetNotificationBatchesHandlerTests.cs
// Mục đích: Kiểm thử validation offset/status và cách tính duration cho batch đang chạy trong danh sách quản lý.

#pragma warning disable CA1707

using NotificationService.Application.Common.Errors;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.UseCases.NotificationBatches.GetList;
using NotificationService.UnitTests.Application.UseCases.NotificationBatches.TestDoubles;

namespace NotificationService.UnitTests.Application.UseCases.NotificationBatches.GetList;

public sealed class GetNotificationBatchesHandlerTests
{
    [Test]
    public async Task HandleAsync_RunningBatch_UsesCurrentTimeForDuration()
    {
        var now = new DateTimeOffset(2026, 8, 18, 10, 0, 10, TimeSpan.Zero);
        var item = new NotificationBatchSummary(
            Guid.NewGuid(), "Batch", "PROCESSING", 10, 5, 5, 0, 500,
            10, null, now.UtcDateTime.AddSeconds(-10), now.UtcDateTime.AddSeconds(-4), null);
        var repository = new StubBatchRepository
        {
            BatchListPage = new NotificationBatchListPage([item], 1, 20, 1),
        };
        var handler = new GetNotificationBatchesHandler(repository, new FixedTimeProvider(now));

        var result = await handler.HandleAsync("processing", 1, 20, CancellationToken.None);

        Assert.That(result.Items.Single().DurationMs, Is.EqualTo(4000));
    }

    [TestCase(0, 20)]
    [TestCase(1, 0)]
    [TestCase(1, 101)]
    public void HandleAsync_InvalidPagination_IsRejected(int page, int pageSize)
    {
        var handler = new GetNotificationBatchesHandler(
            new StubBatchRepository(), TimeProvider.System);

        Assert.ThrowsAsync<NotificationApplicationException>(() =>
            handler.HandleAsync(null, page, pageSize, CancellationToken.None));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
