// File: backend/Services/Ai/AiService.Application/Services/IAiChatClient.cs
// Mục đích: Port gọi một lượt chat completion (có tools, có nội dung ảnh/tệp) tới AI provider OpenAI-compatible.

using System.Text.Json;

namespace AiService.Application.Services;

/// <summary>Một phần nội dung của message: <c>text</c>, <c>image</c> hoặc <c>file</c> (PDF).</summary>
public sealed record AiChatContentPart(string Type, string? Text, string? MediaType, string? DataBase64, string? FileName);

public sealed record AiChatToolCall(string Id, string Name, string ArgumentsJson);

/// <summary>Message gửi cho model; <see cref="Text"/> và <see cref="Parts"/> loại trừ nhau.</summary>
public sealed record AiChatMessage(
    string Role,
    string? Text,
    IReadOnlyList<AiChatContentPart>? Parts,
    IReadOnlyList<AiChatToolCall>? ToolCalls,
    string? ToolCallId);

/// <summary>Định nghĩa tool cho function calling; <see cref="ParametersSchema"/> là JSON Schema của tham số.</summary>
public sealed record AiChatTool(string Name, string Description, JsonElement ParametersSchema);

public sealed record AiChatRequest(
    string ProviderType,
    string BaseUrl,
    string ApiKey,
    string ModelName,
    IReadOnlyList<AiChatMessage> Messages,
    IReadOnlyList<AiChatTool> Tools);

public sealed record AiChatResult(
    string? Text,
    IReadOnlyList<AiChatToolCall> ToolCalls,
    string FinishReason,
    int PromptTokens,
    int CompletionTokens);

public enum AiChatFailureKind
{
    AuthFailed,
    RateLimited,
    Timeout,
    Unreachable,
    Rejected,
    InvalidResponse,
}

/// <summary>Lỗi khi gọi provider; <see cref="Exception.Message"/> đã được làm sạch, không chứa API key hay host nội bộ.</summary>
public sealed class AiChatException(AiChatFailureKind kind, int? httpStatus, string message) : Exception(message)
{
    public AiChatFailureKind Kind { get; } = kind;

    public int? HttpStatus { get; } = httpStatus;
}

public interface IAiChatClient
{
    /// <summary>Gọi provider; ném <see cref="AiChatException"/> cho mọi lỗi mạng/HTTP/định dạng, chỉ ném OperationCanceledException khi <paramref name="cancellationToken"/> bị hủy.</summary>
    Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken);
}
