namespace BuildingBlocks.Contracts.Health;

/// <summary>Abstraction để từng Infrastructure kiểm tra database owner của mình mà Presentation không biết chi tiết kết nối.</summary>
public interface IDatabaseHealthProbe
{
    /// <summary>Thực hiện health check có thể hủy. Kết quả chỉ cho biết dependency healthy hay không.</summary>
    Task<DatabaseHealthProbeResult> CheckAsync(CancellationToken cancellationToken);
}

/// <summary>Kết quả tối giản của database probe.</summary>
public sealed record DatabaseHealthProbeResult(bool IsHealthy);
