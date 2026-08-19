// File: backend/Services/Media/MediaService.UnitTests/TestDoubles/StubNotificationMediaUsageJobRepository.cs
// Mục đích: Ghi nhận lifecycle call của Media Usage job để unit test handler không cần database.

using MediaService.Application.Repositories;

namespace MediaService.UnitTests.TestDoubles;

public sealed class StubNotificationMediaUsageJobRepository : INotificationMediaUsageJobRepository
{
    public Guid? SuccessfulJobId { get; private set; }
    public uint SuccessfulUsageCount { get; private set; }

    public Task<NotificationMediaUsageJobRecord?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken) =>
        Task.FromResult<NotificationMediaUsageJobRecord?>(null);

    public Task StartAsync(Guid jobId, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task RecordSuccessAsync(Guid jobId, uint usageCount, CancellationToken cancellationToken)
    {
        SuccessfulJobId = jobId;
        SuccessfulUsageCount = usageCount;
        return Task.CompletedTask;
    }

    public Task RecordFailureAsync(Guid jobId, uint usageCount, string safeError, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task CompleteSourceAsync(Guid jobId, uint expectedUsageCount, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
