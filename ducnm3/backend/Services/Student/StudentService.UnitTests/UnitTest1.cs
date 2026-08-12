using Microsoft.Extensions.Logging.Abstractions;
using StudentService.Infrastructure.Health;

namespace StudentService.UnitTests;

public class StudentDatabaseHealthProbeTests
{
    [Test]
    public void CheckAsyncPropagatesRequestCancellation()
    {
        var probe = new StudentDatabaseHealthProbe(
            "Server=127.0.0.1;Port=1;Database=student;User Id=user;Password=password;",
            NullLogger<StudentDatabaseHealthProbe>.Instance);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await probe.CheckAsync(cancellationSource.Token));
    }
}
