namespace BuildingBlocks.Contracts.Health;

public interface IMessagingHealthProbe
{
    Task<MessagingHealthProbeResult> CheckAsync(CancellationToken cancellationToken);
}

public sealed record MessagingHealthProbeResult(bool IsHealthy);
