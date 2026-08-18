namespace BuildingBlocks.Contracts.Health;

/// <summary>Abstraction kiểm tra messaging bus để endpoint health không phụ thuộc MassTransit.</summary>
public interface IMessagingHealthProbe
{
    /// <summary>Kiểm tra messaging dependency và trả về trạng thái đã chuẩn hóa.</summary>
    Task<MessagingHealthProbeResult> CheckAsync(CancellationToken cancellationToken);
}

/// <summary>Kết quả tối giản của messaging probe.</summary>
public sealed record MessagingHealthProbeResult(bool IsHealthy);
