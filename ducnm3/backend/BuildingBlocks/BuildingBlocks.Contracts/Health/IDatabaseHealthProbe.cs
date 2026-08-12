namespace BuildingBlocks.Contracts.Health;

public interface IDatabaseHealthProbe
{
    Task<DatabaseHealthProbeResult> CheckAsync(CancellationToken cancellationToken);
}

public sealed record DatabaseHealthProbeResult(bool IsHealthy);
