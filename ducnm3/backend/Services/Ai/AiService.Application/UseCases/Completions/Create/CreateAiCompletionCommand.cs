using AiService.Application.Services;

namespace AiService.Application.UseCases.Completions.Create;

public sealed record CreateAiCompletionCommand(
    string? Task,
    IReadOnlyList<AiChatMessage>? Messages,
    IReadOnlyList<AiChatTool>? Tools);

public sealed record CreateAiCompletionResult(
    string Task,
    string ProviderType,
    string ModelName,
    string? Text,
    IReadOnlyList<AiChatToolCall> ToolCalls,
    string FinishReason,
    int PromptTokens,
    int CompletionTokens,
    long LatencyMs);
