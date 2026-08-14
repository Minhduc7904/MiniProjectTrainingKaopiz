using System;
using System.Collections.Generic;

namespace NotificationService.Infrastructure.Persistence.Scaffolded;

public partial class NotificationBatchItem
{
    /// <summary>
    /// UUID định danh recipient trong batch
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID notification_batches.id
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// UUID Student nhận notification; logical reference
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
    /// Số lần retry item nghiệp vụ đã thực hiện
    /// </summary>
    public uint RetryCount { get; set; }

    /// <summary>
    /// Lỗi cuối cùng; null khi thành công
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Thời điểm xử lý thành công hoặc thất bại cuối, UTC
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// UUID token sở hữu claim PROCESSING hiện tại; null khi item chưa được claim hoặc đã hoàn tất
    /// </summary>
    public Guid? LeaseToken { get; set; }

    /// <summary>
    /// Thời điểm UTC claim PROCESSING hết hạn để worker khác có thể nhận lại
    /// </summary>
    public DateTime? LeaseExpiresAt { get; set; }

    public virtual NotificationBatch Batch { get; set; } = null!;

    public virtual Notification? Notification { get; set; }
}
