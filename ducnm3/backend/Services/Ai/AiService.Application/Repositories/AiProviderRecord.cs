namespace AiService.Application.Repositories;

public sealed record AiProviderRecord(
    Guid Id,
    string Name,
    string ProviderType,
    string BaseUrl,
    string ApiKeyHint,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
