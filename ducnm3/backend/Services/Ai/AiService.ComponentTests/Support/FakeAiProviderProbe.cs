using AiService.Application.Services;

namespace AiService.ComponentTests.Support;

/// <summary>Probe giả: trả kết quả theo base URL đã cấu hình và ghi lại API key đã nhận để test kiểm tra.</summary>
internal sealed class FakeAiProviderProbe : IAiProviderProbe
{
    private readonly Dictionary<string, AiProviderProbeResult> _results = [];

    public List<(string ProviderType, string BaseUrl, string ApiKey)> Calls { get; } = [];

    public AiProviderProbeResult Default { get; set; } = new("ONLINE", 200, 12, "Provider responded successfully.");

    public void Setup(string baseUrl, AiProviderProbeResult result) => _results[baseUrl] = result;

    public Task<AiProviderProbeResult> ProbeAsync(string providerType, string baseUrl, string apiKey, CancellationToken cancellationToken)
    {
        lock (Calls) Calls.Add((providerType, baseUrl, apiKey));
        return Task.FromResult(_results.GetValueOrDefault(baseUrl, Default));
    }
}
