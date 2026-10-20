using System;
using System.Collections.Generic;

namespace AiService.Infrastructure.Persistence.Scaffolded;

public partial class AiModel
{
    /// <summary>
    /// UUID định danh AI model
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID provider sở hữu model
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Tên model gửi cho provider, ví dụ gpt-4o
    /// </summary>
    public string ModelName { get; set; } = null!;

    /// <summary>
    /// Tên hiển thị cho admin
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// ACTIVE | INACTIVE
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo model, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AiServiceBinding> AiServiceBindings { get; set; } = new List<AiServiceBinding>();

    public virtual AiProvider Provider { get; set; } = null!;
}
