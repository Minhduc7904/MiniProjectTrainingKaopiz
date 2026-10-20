namespace AiService.Application.UseCases.Providers.Create;

public sealed record CreateAiProviderCommand(
    string? Name,
    string? ProviderType,
    string? BaseUrl,
    string? ApiKey,
    string? Status);
