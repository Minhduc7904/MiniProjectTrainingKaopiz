namespace AiService.Application.Repositories;

public sealed record AiModelRecord(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    string ProviderType,
    string ModelName,
    string DisplayName,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
