using System;
using System.Collections.Generic;

namespace AiService.Infrastructure.Persistence.Scaffolded;

public partial class AiServiceBinding
{
    /// <summary>
    /// UUID định danh binding
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tên service được dùng AI, ví dụ course-service; logical reference
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// UUID AI model được gán cho service
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Thời điểm tạo binding, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual AiModel Model { get; set; } = null!;
}
