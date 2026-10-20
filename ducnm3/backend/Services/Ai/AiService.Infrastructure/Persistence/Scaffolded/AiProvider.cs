using System;
using System.Collections.Generic;

namespace AiService.Infrastructure.Persistence.Scaffolded;

public partial class AiProvider
{
    /// <summary>
    /// UUID định danh AI provider
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tên provider hiển thị cho admin, không trùng nhau
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// OPENAI | GEMINI | ANTHROPIC | MISTRAL | CUSTOM
    /// </summary>
    public string ProviderType { get; set; } = null!;

    /// <summary>
    /// Endpoint OpenAI-compatible mà OpenAI SDK sẽ gọi tới
    /// </summary>
    public string BaseUrl { get; set; } = null!;

    /// <summary>
    /// API key đã mã hóa AES-GCM, base64 của nonce, tag và ciphertext
    /// </summary>
    public string ApiKeyEncrypted { get; set; } = null!;

    /// <summary>
    /// Gợi ý che key, chỉ chứa 4 ký tự cuối, không dùng để giải mã
    /// </summary>
    public string ApiKeyHint { get; set; } = null!;

    /// <summary>
    /// ACTIVE | INACTIVE
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo provider, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AiModel> AiModels { get; set; } = new List<AiModel>();
}
