namespace AiService.Application.Repositories;

/// <summary>Một dòng nhật ký gọi model: chỉ số vận hành, không chứa nội dung prompt hay API key.</summary>
public sealed record AiInvocationRecord(
    Guid Id,
    string TaskName,
    Guid ModelId,
    string ProviderType,
    string ModelName,
    string Status,
    string? ErrorCode,
    int PromptTokens,
    int CompletionTokens,
    int ToolCallCount,
    long LatencyMs,
    DateTime CreatedAtUtc);
