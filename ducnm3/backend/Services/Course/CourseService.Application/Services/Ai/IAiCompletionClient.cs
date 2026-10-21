// File: backend/Services/Course/CourseService.Application/Services/Ai/IAiCompletionClient.cs
// Mục đích: Port gọi AI Service để chạy một lượt chat completion (có tools, ảnh, PDF) cho một tác vụ AI.

using System.Text.Json;

namespace CourseService.Application.Services.Ai;

/// <summary>Một phần nội dung message: <c>text</c>, <c>image</c> hoặc <c>file</c> (PDF).</summary>
public sealed record AiContentPart(string Type, string? Text, string? MediaType, string? DataBase64, string? FileName)
{
    public static AiContentPart FromText(string text) => new("text", text, null, null, null);
}

public sealed record AiToolCall(string Id, string Name, string ArgumentsJson);

public sealed record AiMessage(
    string Role,
    string? Text,
    IReadOnlyList<AiContentPart>? Parts = null,
    IReadOnlyList<AiToolCall>? ToolCalls = null,
    string? ToolCallId = null);

public sealed record AiToolDefinition(string Name, string Description, JsonElement ParametersSchema);

public sealed record AiCompletion(
    string? Text,
    IReadOnlyList<AiToolCall> ToolCalls,
    string FinishReason,
    int PromptTokens,
    int CompletionTokens);

/// <summary>Lỗi từ AI Service; <see cref="Code"/> là error code ổn định, <see cref="Exception.Message"/> đã an toàn để hiển thị.</summary>
public sealed class AiCompletionException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public interface IAiCompletionClient
{
    /// <param name="task">Khóa binding AI, ví dụ <c>document-extraction</c>.</param>
    /// <param name="actorId">Admin được ghi nhận là actor của lời gọi.</param>
    Task<AiCompletion> CompleteAsync(
        string task,
        IReadOnlyList<AiMessage> messages,
        IReadOnlyList<AiToolDefinition> tools,
        Guid actorId,
        CancellationToken cancellationToken);
}
