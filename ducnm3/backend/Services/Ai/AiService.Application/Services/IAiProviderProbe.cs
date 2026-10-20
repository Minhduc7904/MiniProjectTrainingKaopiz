// File: backend/Services/Ai/AiService.Application/Services/IAiProviderProbe.cs
// Mục đích: Port gọi thử một API nhẹ của AI provider để biết base URL và API key còn dùng được không.

namespace AiService.Application.Services;

public sealed record AiProviderProbeResult(string Status, int? HttpStatus, long LatencyMs, string Message);

public interface IAiProviderProbe
{
    /// <summary>Gọi thử provider; mọi lỗi mạng/HTTP đều được gói trong kết quả, chỉ ném khi <paramref name="cancellationToken"/> bị hủy.</summary>
    Task<AiProviderProbeResult> ProbeAsync(string providerType, string baseUrl, string apiKey, CancellationToken cancellationToken);
}
