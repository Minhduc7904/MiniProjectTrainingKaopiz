namespace AiService.Application.UseCases.Models.Update;

/// <summary>Provider của model không đổi được; muốn đổi provider thì tạo model mới.</summary>
public sealed record UpdateAiModelCommand(
    Guid ModelId,
    string? ModelName,
    string? DisplayName,
    string? Status);
