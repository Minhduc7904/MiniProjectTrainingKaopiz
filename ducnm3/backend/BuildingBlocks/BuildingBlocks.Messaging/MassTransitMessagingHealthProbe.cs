using BuildingBlocks.Contracts.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Messaging;

public sealed class MassTransitMessagingHealthProbe(HealthCheckService healthCheckService)
    : IMessagingHealthProbe
{
    private const string MassTransitHealthTag = "masstransit";

    public async Task<MessagingHealthProbeResult> CheckAsync(
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(
            registration => registration.Tags.Contains(MassTransitHealthTag),
            cancellationToken);

        return new MessagingHealthProbeResult(
            report.Entries.Count > 0 && report.Status == HealthStatus.Healthy);
    }
}
