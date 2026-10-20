namespace AiService.Api.Contracts.Providers;

/// <summary>Kết quả thử kết nối provider; <c>LatencyMs</c> null khi provider tắt. Không chứa API key.</summary>
public sealed record AiProviderHealthResponse(
    Guid ProviderId,
    string Name,
    string ProviderType,
    string BaseUrl,
    string ProviderStatus,
    string HealthStatus,
    int? HttpStatus,
    long? LatencyMs,
    string Message,
    DateTime CheckedAtUtc);
