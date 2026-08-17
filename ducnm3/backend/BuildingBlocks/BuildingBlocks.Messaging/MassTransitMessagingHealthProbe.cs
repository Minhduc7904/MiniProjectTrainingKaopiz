using BuildingBlocks.Contracts.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Messaging;

/// <summary>Adapter biến MassTransit health check thành contract trung lập cho endpoint <c>/health</c>.</summary>
public sealed class MassTransitMessagingHealthProbe(HealthCheckService healthCheckService)
    : IMessagingHealthProbe
{
    private const string MassTransitHealthTag = "masstransit";

    /// <summary>Chỉ chạy health check có tag <c>masstransit</c>; trả unhealthy khi không có entry hoặc có entry lỗi.</summary>
    public async Task<MessagingHealthProbeResult> CheckAsync(
        CancellationToken cancellationToken)
    {
        // Lọc tag để database/storage health check không ảnh hưởng kết luận của messaging probe.
        var report = await healthCheckService.CheckHealthAsync(
            registration => registration.Tags.Contains(MassTransitHealthTag),
            cancellationToken);

        return new MessagingHealthProbeResult(
            report.Entries.Count > 0 && report.Status == HealthStatus.Healthy);
    }
}
