using Microsoft.Extensions.Logging.Abstractions;
using NotificationService.Infrastructure.Health;

namespace NotificationService.UnitTests;

public class NotificationDatabaseHealthProbeTests
{
    [Test]
    public void CheckAsyncPropagatesRequestCancellation()
    {
        var probe = new NotificationDatabaseHealthProbe(
            "Server=127.0.0.1;Port=1;Database=notification;User Id=user;Password=password;",
            NullLogger<NotificationDatabaseHealthProbe>.Instance);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await probe.CheckAsync(cancellationSource.Token));
    }
}
