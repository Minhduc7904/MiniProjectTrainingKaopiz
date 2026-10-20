namespace AiService.Api.Contracts.Options;

public sealed record AiProviderTypeOptionResponse(string ProviderType, string? DefaultBaseUrl);

public sealed record AiOptionsResponse(
    IReadOnlyList<AiProviderTypeOptionResponse> ProviderTypes,
    IReadOnlyList<string> BindableServices);
