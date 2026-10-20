namespace AiService.Application.UseCases.Models.Create;

public sealed record CreateAiModelCommand(
    Guid? ProviderId,
    string? ModelName,
    string? DisplayName,
    string? Status);
