namespace AiService.Api.Contracts.Providers;

public sealed record CreateAiProviderRequest(
    string? Name,
    string? ProviderType,
    string? BaseUrl,
    string? ApiKey,
    string? Status);

/// <summary><see cref="ApiKey"/> bỏ trống nghĩa là giữ nguyên key đã lưu.</summary>
public sealed record UpdateAiProviderRequest(
    string? Name,
    string? ProviderType,
    string? BaseUrl,
    string? ApiKey,
    string? Status);
