namespace AiService.Api.Contracts.Providers;

/// <summary>Provider trả về cho client; chỉ có hint che key, không bao giờ có API key hoặc ciphertext.</summary>
public sealed record AiProviderResponse(
    Guid Id,
    string Name,
    string ProviderType,
    string BaseUrl,
    string ApiKeyHint,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
