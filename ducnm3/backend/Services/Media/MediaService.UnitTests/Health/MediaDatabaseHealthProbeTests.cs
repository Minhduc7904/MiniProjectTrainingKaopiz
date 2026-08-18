// File: backend/Services/Media/MediaService.UnitTests/Health/MediaDatabaseHealthProbeTests.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Infrastructure.Health;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediaService.UnitTests.Health;

public class MediaDatabaseHealthProbeTests
{
    [Test]
    public void CheckAsyncPropagatesRequestCancellation()
    {
        var probe = new MediaDatabaseHealthProbe(
            "Server=127.0.0.1;Port=1;Database=media;User Id=user;Password=password;",
            NullLogger<MediaDatabaseHealthProbe>.Instance);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await probe.CheckAsync(cancellationSource.Token));
    }
}
