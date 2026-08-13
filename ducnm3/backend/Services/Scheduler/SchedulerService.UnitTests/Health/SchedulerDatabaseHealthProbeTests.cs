using Microsoft.Extensions.Logging.Abstractions;
using SchedulerService.Infrastructure.Health;

namespace SchedulerService.UnitTests.Health;

public class SchedulerDatabaseHealthProbeTests
{
    [Test]
    public void CheckAsyncPropagatesRequestCancellation()
    {
        var probe = new SchedulerDatabaseHealthProbe(
            "Server=127.0.0.1;Port=1;Database=scheduler;User Id=user;Password=password;",
            NullLogger<SchedulerDatabaseHealthProbe>.Instance);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await probe.CheckAsync(cancellationSource.Token));
    }
}
