using CourseService.Infrastructure.Health;
using Microsoft.Extensions.Logging.Abstractions;

namespace CourseService.UnitTests;

public class CourseDatabaseHealthProbeTests
{
    [Test]
    public void CheckAsyncPropagatesRequestCancellation()
    {
        var probe = new CourseDatabaseHealthProbe(
            "Server=127.0.0.1;Port=1;Database=course;User Id=user;Password=password;",
            NullLogger<CourseDatabaseHealthProbe>.Instance);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await probe.CheckAsync(cancellationSource.Token));
    }
}
