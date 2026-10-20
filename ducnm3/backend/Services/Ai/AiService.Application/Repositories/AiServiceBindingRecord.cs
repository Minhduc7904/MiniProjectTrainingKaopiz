namespace AiService.Application.Repositories;

public sealed record AiServiceBindingRecord(
    Guid Id,
    string ServiceName,
    Guid ModelId,
    string ModelName,
    string ModelDisplayName,
    string ModelStatus,
    Guid ProviderId,
    string ProviderName,
    string ProviderType,
    string ProviderStatus,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
