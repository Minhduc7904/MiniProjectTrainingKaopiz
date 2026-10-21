using System;
using System.Collections.Generic;

namespace AiService.Infrastructure.Persistence.Scaffolded;

public partial class AiInvocation
{
    /// <summary>
    /// UUID định danh lần gọi model
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Khóa binding đã gọi, ví dụ document-extraction; logical reference tới ai_service_bindings.service_name
    /// </summary>
    public string TaskName { get; set; } = null!;

    /// <summary>
    /// UUID AI model đã dùng; logical reference, không có khóa ngoại để model xóa được mà vẫn giữ nhật ký
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Loại provider tại thời điểm gọi: OPENAI | GEMINI | ANTHROPIC | MISTRAL | CUSTOM
    /// </summary>
    public string ProviderType { get; set; } = null!;

    /// <summary>
    /// Tên model gửi cho provider tại thời điểm gọi
    /// </summary>
    public string ModelName { get; set; } = null!;

    /// <summary>
    /// SUCCESS | FAILED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Mã lỗi AI_PROVIDER_* khi FAILED, NULL khi SUCCESS
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Số token đầu vào provider báo; 0 khi lỗi hoặc provider không báo
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Số token đầu ra provider báo; 0 khi lỗi hoặc provider không báo
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Số tool call model yêu cầu trong lượt này
    /// </summary>
    public int ToolCallCount { get; set; }

    /// <summary>
    /// Thời gian gọi provider tính bằng mili giây
    /// </summary>
    public long LatencyMs { get; set; }

    /// <summary>
    /// Thời điểm gọi, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
