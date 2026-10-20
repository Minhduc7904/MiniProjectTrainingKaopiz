namespace AiService.Api.Contracts.Models;

public sealed record CreateAiModelRequest(
    Guid? ProviderId,
    string? ModelName,
    string? DisplayName,
    string? Status);

public sealed record UpdateAiModelRequest(
    string? ModelName,
    string? DisplayName,
    string? Status);
