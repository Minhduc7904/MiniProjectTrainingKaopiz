// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/MediaUsageJobs/GetStatus/GetNotificationMediaUsageJobStatusHandlerTests.cs
// Mục đích: Kiểm thử status API của background job đăng ký Media Usage từ Markdown Notification Batch.

#pragma warning disable CA1707

using MediaService.Application.Repositories;
using MediaService.Application.UseCases.MediaUsageJobs.GetStatus;

namespace MediaService.UnitTests.Application.UseCases.MediaUsageJobs.GetStatus;

public sealed class GetNotificationMediaUsageJobStatusHandlerTests
{
    private static readonly Guid JobId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public async Task HandleAsync_ProcessingJob_ReturnsHandledProgress()
    {
        var repository = new StubJobRepository(new NotificationMediaUsageJobRecord(
            JobId, "PROCESSING", 1000, 600, 10,
            DateTime.UnixEpoch, DateTime.UnixEpoch.AddSeconds(1), null, null));
        var sut = new GetNotificationMediaUsageJobStatusHandler(repository);

        var result = await sut.HandleAsync(JobId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ProgressPercent, Is.EqualTo(61m));
            Assert.That(result.RemainingUsageCount, Is.EqualTo(390));
            Assert.That(result.Status, Is.EqualTo("PROCESSING"));
        });
    }

    private sealed class StubJobRepository(NotificationMediaUsageJobRecord? record)
        : INotificationMediaUsageJobRepository
    {
        public Task<NotificationMediaUsageJobRecord?> GetByIdAsync(
            Guid jobId,
            CancellationToken cancellationToken) => Task.FromResult(record);

        public Task StartAsync(Guid jobId, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task RecordSuccessAsync(Guid jobId, uint usageCount, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task RecordFailureAsync(Guid jobId, uint usageCount, string safeError, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task CompleteSourceAsync(Guid jobId, uint expectedUsageCount, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
