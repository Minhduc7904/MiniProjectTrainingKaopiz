namespace AiService.Api.Contracts.Models;

public sealed record AiModelResponse(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    string ProviderType,
    string ModelName,
    string DisplayName,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
