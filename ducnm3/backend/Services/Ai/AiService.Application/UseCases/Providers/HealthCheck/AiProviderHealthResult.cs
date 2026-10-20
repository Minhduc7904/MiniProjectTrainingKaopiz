using AiService.Application.Repositories;

namespace AiService.Application.UseCases.Providers.HealthCheck;

/// <summary>Kết quả thử kết nối của một provider; <see cref="LatencyMs"/> null khi provider tắt nên không gọi ra ngoài.</summary>
public sealed record AiProviderHealthResult(
    AiProviderRecord Provider,
    string HealthStatus,
    int? HttpStatus,
    long? LatencyMs,
    string Message,
    DateTime CheckedAtUtc);
