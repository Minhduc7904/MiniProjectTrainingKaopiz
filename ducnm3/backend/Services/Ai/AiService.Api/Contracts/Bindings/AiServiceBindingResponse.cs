namespace AiService.Api.Contracts.Bindings;

public sealed record AiServiceBindingResponse(
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

public sealed record UpsertAiServiceBindingRequest(Guid? ModelId);
