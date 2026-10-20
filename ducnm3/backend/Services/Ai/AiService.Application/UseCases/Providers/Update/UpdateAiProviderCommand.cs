namespace AiService.Application.UseCases.Providers.Update;

/// <summary><see cref="ApiKey"/> rỗng hoặc null nghĩa là giữ nguyên key đã lưu.</summary>
public sealed record UpdateAiProviderCommand(
    Guid ProviderId,
    string? Name,
    string? ProviderType,
    string? BaseUrl,
    string? ApiKey,
    string? Status);
