// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Scaffolded/MediaDerivationJob.cs
// Mục đích: Mô hình EF Core sinh từ database; chỉ phản ánh schema và không chứa business logic viết tay.

﻿using System;
using System.Collections.Generic;

namespace MediaService.Infrastructure.Persistence.Scaffolded;

public partial class MediaDerivationJob
{
    /// <summary>
    /// UUID định danh operation tạo media dẫn xuất
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Media gốc cần tạo thumbnail
    /// </summary>
    public Guid SourceMediaId { get; set; }

    /// <summary>
    /// Media WebP dẫn xuất được cấp trước
    /// </summary>
    public Guid DerivativeMediaId { get; set; }

    /// <summary>
    /// Loại dẫn xuất; hiện chỉ hỗ trợ THUMBNAIL
    /// </summary>
    public string DerivationType { get; set; } = null!;

    /// <summary>
    /// QUEUED | PROCESSING | READY | FAILED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Số lần worker đã bắt đầu xử lý
    /// </summary>
    public uint AttemptCount { get; set; }

    /// <summary>
    /// Lỗi an toàn của lần xử lý cuối; không trả chi tiết nội bộ
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Thời điểm job được tạo, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm lần xử lý gần nhất bắt đầu, UTC
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Thời điểm job đạt trạng thái terminal, UTC
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Thời điểm job cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual MediaObject DerivativeMedia { get; set; } = null!;

    public virtual MediaObject SourceMedia { get; set; } = null!;
}
