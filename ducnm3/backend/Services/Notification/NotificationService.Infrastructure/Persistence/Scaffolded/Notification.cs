// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Scaffolded/Notification.cs
// Mục đích: Entity EF scaffolded Notification ánh xạ bảng database hiện có; phục vụ persistence và không chứa nghiệp vụ use case.

﻿using System;
using System.Collections.Generic;

namespace NotificationService.Infrastructure.Persistence.Scaffolded;

public partial class Notification
{
    /// <summary>
    /// UUID định danh inbox item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Student sở hữu notification; logical reference
    /// </summary>
    public Guid RecipientStudentId { get; set; }

    /// <summary>
    /// Tiêu đề hiển thị trong inbox
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Nội dung Markdown; media nhúng dùng URL Media Service
    /// </summary>
    public string BodyMarkdown { get; set; } = null!;

    /// <summary>
    /// SINGLE | BULK
    /// </summary>
    public string SourceType { get; set; } = null!;

    /// <summary>
    /// UUID notification_batches.id; null với gửi đơn
    /// </summary>
    public Guid? NotificationBatchId { get; set; }

    /// <summary>
    /// UUID admin hoặc system tạo notification; logical reference
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// UNREAD | READ
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Thời điểm recipient đánh dấu đã đọc; null khi UNREAD
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Thời điểm notification xuất hiện trong inbox, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual NotificationBatch? NotificationBatch { get; set; }

    public virtual ICollection<NotificationBatchItem> NotificationBatchItems { get; set; } = new List<NotificationBatchItem>();
}
