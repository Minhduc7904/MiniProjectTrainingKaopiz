using System.Text.Json;

namespace AiService.Api.Contracts.Completions;

/// <summary>Một phần nội dung message: <c>text</c>, <c>image</c> hoặc <c>file</c> (PDF); ảnh và tệp gửi dạng base64.</summary>
public sealed record AiCompletionContentPartRequest(
    string? Type,
    string? Text,
    string? MediaType,
    string? DataBase64,
    string? FileName);

public sealed record AiCompletionToolCallRequest(string? Id, string? Name, string? ArgumentsJson);

/// <summary><c>Text</c> và <c>Parts</c> loại trừ nhau; assistant có thể kèm <c>ToolCalls</c>, message role tool cần <c>ToolCallId</c>.</summary>
public sealed record AiCompletionMessageRequest(
    string? Role,
    string? Text,
    IReadOnlyList<AiCompletionContentPartRequest>? Parts,
    IReadOnlyList<AiCompletionToolCallRequest>? ToolCalls,
    string? ToolCallId);

public sealed record AiCompletionToolRequest(string? Name, string? Description, JsonElement ParametersSchema);

/// <summary><c>Task</c> là khóa binding (tác vụ AI hoặc service) dùng để chọn model.</summary>
public sealed record CreateAiCompletionRequest(
    string? Task,
    IReadOnlyList<AiCompletionMessageRequest>? Messages,
    IReadOnlyList<AiCompletionToolRequest>? Tools);

public sealed record AiCompletionToolCallResponse(string Id, string Name, string ArgumentsJson);

public sealed record AiCompletionResponse(
    string Task,
    string ProviderType,
    string ModelName,
    string? Text,
    IReadOnlyList<AiCompletionToolCallResponse> ToolCalls,
    string FinishReason,
    int PromptTokens,
    int CompletionTokens,
    long LatencyMs);
