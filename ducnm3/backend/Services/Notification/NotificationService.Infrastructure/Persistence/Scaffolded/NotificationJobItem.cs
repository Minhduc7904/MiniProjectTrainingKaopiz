using System;
using System.Collections.Generic;

namespace NotificationService.Infrastructure.Persistence.Scaffolded;

public partial class NotificationJobItem
{
    /// <summary>
    /// UUID định danh recipient trong batch
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID notification_jobs.id
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// UUID Student nhận thông báo; logical reference
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// UUID notifications.id được tạo; null khi chưa thành công
    /// </summary>
    public Guid? NotificationId { get; set; }

    /// <summary>
    /// PENDING | PROCESSING | SUCCESS | RETRY | FAILED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Số lần retry đã thực hiện
    /// </summary>
    public uint RetryCount { get; set; }

    /// <summary>
    /// Lỗi cuối cùng; null khi thành công
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Thời điểm xử lý thành công hoặc thất bại cuối; null khi chưa xử lý
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    public virtual NotificationJob Job { get; set; } = null!;

    public virtual Notification? Notification { get; set; }
}
