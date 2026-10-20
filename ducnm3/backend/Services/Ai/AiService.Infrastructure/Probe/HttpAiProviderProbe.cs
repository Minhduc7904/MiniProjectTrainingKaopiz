// File: backend/Services/Ai/AiService.Infrastructure/Probe/HttpAiProviderProbe.cs
// Mục đích: Gọi GET {baseUrl}/models bằng API key để kiểm tra kết nối tới AI provider OpenAI-compatible.

using System.Diagnostics;
using AiService.Application.Services;
using AiService.Domain.Constants;

namespace AiService.Infrastructure.Probe;

public sealed class HttpAiProviderProbe(HttpClient httpClient) : IAiProviderProbe
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(6);

    private const string ModelsPath = "models";
    private const string AnthropicVersion = "2023-06-01";

    public async Task<AiProviderProbeResult> ProbeAsync(string providerType, string baseUrl, string apiKey, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(baseUrl.TrimEnd('/') + "/" + ModelsPath, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return new AiProviderProbeResult(AiProviderHealthStatuses.Error, null, 0, "Base URL is not a valid http(s) address.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");
        if (providerType == AiProviderTypes.Anthropic)
        {
            request.Headers.TryAddWithoutValidation("x-api-key", apiKey);
            request.Headers.TryAddWithoutValidation("anthropic-version", AnthropicVersion);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Timeout);
            // Chỉ cần status code nên dừng ngay khi nhận header, không tải body.
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            return Classify((int)response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new AiProviderProbeResult(AiProviderHealthStatuses.Unreachable, null, stopwatch.ElapsedMilliseconds, "Provider did not respond in time.");
        }
        catch (HttpRequestException)
        {
            // Không đưa message của exception ra ngoài: có thể chứa host nội bộ.
            return new AiProviderProbeResult(AiProviderHealthStatuses.Unreachable, null, stopwatch.ElapsedMilliseconds, "Cannot connect to the provider base URL.");
        }
    }

    private static AiProviderProbeResult Classify(int statusCode, long latencyMs) =>
        statusCode switch
        {
            >= 200 and < 300 => new(AiProviderHealthStatuses.Online, statusCode, latencyMs, "Provider responded successfully."),
            401 or 403 => new(AiProviderHealthStatuses.AuthFailed, statusCode, latencyMs, "Provider rejected the API key."),
            _ => new(AiProviderHealthStatuses.Error, statusCode, latencyMs, $"Provider returned HTTP {statusCode}."),
        };
}
